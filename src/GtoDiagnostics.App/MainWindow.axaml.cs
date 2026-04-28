using Avalonia.Controls;
using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Serial;
using GtoDiagnostics.Simulator;

namespace GtoDiagnostics.App;

public partial class MainWindow : Window
{
    private readonly LinuxSerialPortDiscovery portDiscovery = new();
    private readonly VehicleModuleDefinition engineDefinition = KnownModuleDefinitions.CreateProvisionalEngineEcu();
    private readonly LiveDataDecoder liveDataDecoder;
    private RawCaptureWriter? captureWriter;
    private string? capturePath;
    private int sampleCount;

    public MainWindow()
    {
        liveDataDecoder = new LiveDataDecoder(engineDefinition);

        InitializeComponent();

        RefreshPortsButton.Click += (_, _) => RefreshPorts();
        TestConnectionButton.Click += (_, _) => TestConnection();
        StartCaptureButton.Click += async (_, _) => await StartCaptureAsync();
        StopCaptureButton.Click += async (_, _) => await StopCaptureAsync();
        SimulateSampleButton.Click += async (_, _) => await SimulateSampleAsync();
        SimulatorCheckBox.IsCheckedChanged += (_, _) => UpdateSessionSummary();

        RefreshPorts();
        UpdateSessionSummary();
        SensorReadingsListBox.ItemsSource = new[] { "No decoded readings yet." };
    }

    private void RefreshPorts()
    {
        var ports = portDiscovery.ListPorts();
        PortComboBox.ItemsSource = ports.Select(static port => port.Name).ToArray();

        if (ports.Count > 0)
        {
            PortComboBox.SelectedIndex = 0;
            AppendLog($"Found {ports.Count} serial device(s).");
        }
        else
        {
            PortComboBox.SelectedIndex = -1;
            AppendLog("No Linux USB serial devices found.");
        }
    }

    private void TestConnection()
    {
        if (SimulatorCheckBox.IsChecked == true)
        {
            ConnectionStatusText.Text = "Simulator";
            AppendLog("Simulator transport is available.");
            return;
        }

        var portName = PortComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(portName))
        {
            ConnectionStatusText.Text = "No port";
            AppendLog("No serial port selected.");
            return;
        }

        ConnectionStatusText.Text = "Port selected";
        AppendLog($"Selected {portName}.");
    }

    private async Task StartCaptureAsync()
    {
        if (captureWriter is not null)
        {
            return;
        }

        Directory.CreateDirectory("captures");
        capturePath = Path.Combine("captures", $"session-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.jsonl");
        captureWriter = new RawCaptureWriter(capturePath);

        CapturePathText.Text = capturePath;
        StartCaptureButton.IsEnabled = false;
        StopCaptureButton.IsEnabled = true;

        await WriteCaptureMessageAsync(RawMessageDirection.Transmit, "capture_start"u8.ToArray());
        AppendLog($"Capture started: {capturePath}");
    }

    private async Task StopCaptureAsync()
    {
        if (captureWriter is null)
        {
            return;
        }

        await WriteCaptureMessageAsync(RawMessageDirection.Receive, "capture_stop"u8.ToArray());
        await captureWriter.DisposeAsync();
        captureWriter = null;

        StartCaptureButton.IsEnabled = true;
        StopCaptureButton.IsEnabled = false;
        AppendLog("Capture stopped.");
    }

    private async Task SimulateSampleAsync()
    {
        await using var transport = new ScriptedByteTransport();
        var command = HexBytes.Parse("10 01");
        var response = HexBytes.Parse("90 01 64 80 8E");

        transport.EnqueueResponse(response);
        await transport.OpenAsync();
        await transport.WriteAsync(command);
        await WriteCaptureMessageAsync(RawMessageDirection.Transmit, command);

        var buffer = new byte[16];
        var count = await transport.ReadAsync(buffer);
        var received = buffer[..count];
        await WriteCaptureMessageAsync(RawMessageDirection.Receive, received);

        sampleCount++;
        UpdateRateText.Text = $"{sampleCount} sample(s)";
        ConnectionStatusText.Text = "Simulator";
        SensorReadingsListBox.ItemsSource = liveDataDecoder
            .Decode(received)
            .Select(FormatReading)
            .ToArray();
        AppendLog($"TX {HexBytes.Format(command)}");
        AppendLog($"RX {HexBytes.Format(received)}");
    }

    private static string FormatReading(SensorReading reading)
    {
        return $"{reading.Name}: {reading.Value:0.##} {reading.Unit}";
    }

    private async Task WriteCaptureMessageAsync(RawMessageDirection direction, byte[] bytes)
    {
        if (captureWriter is null)
        {
            return;
        }

        await captureWriter.WriteAsync(new RawDiagnosticMessage(
            DateTimeOffset.UtcNow,
            direction,
            DiagnosticModule.EngineEcu.ToString(),
            bytes));
    }

    private void UpdateSessionSummary()
    {
        SessionSummaryText.Text = SimulatorCheckBox.IsChecked == true
            ? "Simulator ready"
            : "Waiting for serial connection";
    }

    private void AppendLog(string message)
    {
        var timestamp = DateTimeOffset.Now.ToString("HH:mm:ss");
        RawLogTextBox.Text = $"{RawLogTextBox.Text}{Environment.NewLine}[{timestamp}] {message}";
        RawLogTextBox.CaretIndex = RawLogTextBox.Text?.Length ?? 0;
    }
}
