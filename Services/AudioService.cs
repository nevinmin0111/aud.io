using System.Collections.ObjectModel;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SoundDesk.Models;

namespace SoundDesk.Services;

public sealed class AudioService : IMMNotificationClient, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    public ObservableCollection<OutputDevice> Devices { get; } = new();
    public ObservableCollection<AppAudioSession> Sessions { get; } = new();
    public event EventHandler? AudioChanged;

    public AudioService()
    {
        _enumerator.RegisterEndpointNotificationCallback(this);
        Refresh();
    }

    public void Refresh()
    {
        string? defaultId = null;
        try { defaultId = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID; }
        catch { }

        var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
        var existingDevices = Devices.ToDictionary(d => d.Id);
        foreach (var old in Devices.Where(d => devices.All(n => n.ID != d.Id)).ToList()) Devices.Remove(old);
        foreach (var device in devices)
        {
            if (!existingDevices.TryGetValue(device.ID, out var model)) Devices.Add(new OutputDevice(device, device.ID == defaultId));
            else model.IsDefault = device.ID == defaultId;
        }

        var found = new Dictionary<string, AppAudioSession>();
        foreach (var output in Devices)
        {
            AudioSessionManager sessions;
            try { sessions = output.Device.AudioSessionManager; }
            catch { continue; }

            for (var i = 0; i < sessions.Sessions.Count; i++)
            {
                var session = sessions.Sessions[i];
                if (session.State == AudioSessionState.AudioSessionStateExpired) continue;
                var pid = unchecked((int)session.GetProcessID);
                var key = $"{output.Id}|{pid}|{session.GetSessionIdentifier}";
                var name = GetAppName(session, pid);
                found[key] = new AppAudioSession(session, key, name, output.Name, pid);
            }
        }

        foreach (var old in Sessions.Where(s => !found.ContainsKey(s.Key)).ToList()) Sessions.Remove(old);
        var existingSessions = Sessions.ToDictionary(s => s.Key);
        foreach (var item in found.Values)
            if (!existingSessions.ContainsKey(item.Key)) Sessions.Add(item);

    }

    public void MakeDefault(OutputDevice device)
    {
        DefaultDeviceService.SetDefault(device.Id);
        Refresh();
    }

    private static string GetAppName(AudioSessionControl session, int pid)
    {
        if (!string.IsNullOrWhiteSpace(session.DisplayName)) return session.DisplayName;
        if (pid == 0) return "Windows system sounds";
        try
        {
            var process = Process.GetProcessById(pid);
            return process.MainModule?.FileVersionInfo.FileDescription
                   ?? process.ProcessName;
        }
        catch { return $"App {pid}"; }
    }

    public void Dispose()
    {
        _enumerator.UnregisterEndpointNotificationCallback(this);
        _enumerator.Dispose();
        foreach (var d in Devices) d.Device.Dispose();
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState) => AudioChanged?.Invoke(this, EventArgs.Empty);
    public void OnDeviceAdded(string pwstrDeviceId) => AudioChanged?.Invoke(this, EventArgs.Empty);
    public void OnDeviceRemoved(string deviceId) => AudioChanged?.Invoke(this, EventArgs.Empty);
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) => AudioChanged?.Invoke(this, EventArgs.Empty);
    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) => AudioChanged?.Invoke(this, EventArgs.Empty);
}
