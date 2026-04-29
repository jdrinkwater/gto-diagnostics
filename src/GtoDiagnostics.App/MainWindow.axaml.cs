using Avalonia.Controls;
using GtoDiagnostics.Core;
using GtoDiagnostics.Core.Definitions;
using GtoDiagnostics.Protocol;
using GtoDiagnostics.Runtime;
using GtoDiagnostics.Serial;
using GtoDiagnostics.Simulator;

namespace GtoDiagnostics.App;

public partial class MainWindow : Window
{
    private readonly LinuxSerialPortDiscovery portDiscovery = new();
    private readonly VehicleModuleDefinition engineDefinition = KnownModuleDefinitions.CreateProvisionalEngineEcu();
    private DiagnosticCaptureSession? captureSession;
    private int sampleCount;

    public MainWindow()
    {
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
        if (captureSession is not null)
        {
            return;
        }

        captureSession = await DiagnosticCaptureSession.CreateAsync(
            "captures",
            engineDefinition,
            SimulatorCheckBox.IsChecked == true ? "Simulator" : "Serial");
        var manifest = captureSession.Manifest;

        CapturePathText.Text = $"Manifest: {manifest.ManifestPath}{Environment.NewLine}Raw: {manifest.RawCapturePath}{Environment.NewLine}Readings: {manifest.DecodedReadingsPath}{Environment.NewLine}CSV: {manifest.DecodedCsvPath}";
        StartCaptureButton.IsEnabled = false;
        StopCaptureButton.IsEnabled = true;

        await WriteCaptureMessageAsync(RawMessageDirection.Transmit, "capture_start"u8.ToArray());
        AppendLog($"Manifest written: {manifest.ManifestPath}");
        AppendLog($"Capture started: {manifest.RawCapturePath}");
        AppendLog($"Decoded readings log started: {manifest.DecodedReadingsPath}");
        AppendLog($"Decoded CSV log started: {manifest.DecodedCsvPath}");
    }

    private async Task StopCaptureAsync()
    {
        if (captureSession is null)
        {
            return;
        }

        await WriteCaptureMessageAsync(RawMessageDirection.Receive, "capture_stop"u8.ToArray());
        await captureSession.DisposeAsync();
        captureSession = null;

        StartCaptureButton.IsEnabled = true;
        StopCaptureButton.IsEnabled = false;
        AppendLog("Capture stopped.");
    }

    private async Task SimulateSampleAsync()
    {
        await using var transport = new ScriptedByteTransport();
        var response = HexBytes.Parse("90 01 64 80 8E");

        transport.EnqueueResponse(response);
        await transport.OpenAsync();
        var pollingSession = new LivePollingSession(
            transport,
            engineDefinition,
            captureSession?.RawCaptureWriter,
            captureSession?.DecodedReadingLogWriter,
            captureSession?.DecodedReadingCsvLogWriter);
        var result = await pollingSession.PollOnceAsync();

        sampleCount++;
        UpdateRateText.Text = $"{sampleCount} sample(s)";
        ConnectionStatusText.Text = "Simulator";
        SensorReadingsListBox.ItemsSource = result.Readings.Select(FormatReading).ToArray();
        AppendLog($"TX {HexBytes.Format(result.Command)}");
        AppendLog($"RX {HexBytes.Format(result.Response)}");
    }

    private static string FormatReading(SensorReading reading)
    {
        return $"{reading.Name}: {reading.Value:0.##} {reading.Unit}";
    }

    private async Task WriteCaptureMessageAsync(RawMessageDirection direction, byte[] bytes)
    {
        if (captureSession is null)
        {
            return;
        }

        await captureSession.WriteRawMessageAsync(direction, bytes);
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
