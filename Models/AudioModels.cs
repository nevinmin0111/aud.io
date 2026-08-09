using System.ComponentModel;
using System.Runtime.CompilerServices;
using NAudio.CoreAudioApi;

namespace SoundDesk.Models;

public abstract class NotifyModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class OutputDevice : NotifyModel
{
    internal MMDevice Device { get; }
    public string Id => Device.ID;
    public string Name => Device.FriendlyName;
    public string StateText => Device.State.ToString();

    private bool _isDefault;
    public bool IsDefault { get => _isDefault; set { _isDefault = value; Changed(); } }

    public float Volume
    {
        get => Device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f;
        set { Device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(value / 100f, 0f, 1f); Changed(); }
    }

    public bool IsMuted
    {
        get => Device.AudioEndpointVolume.Mute;
        set { Device.AudioEndpointVolume.Mute = value; Changed(); }
    }

    internal OutputDevice(MMDevice device, bool isDefault)
    {
        Device = device;
        _isDefault = isDefault;
    }
}

public sealed class AppAudioSession : NotifyModel
{
    internal AudioSessionControl Session { get; }
    public string Key { get; }
    public string AppName { get; }
    public string DeviceName { get; }
    public int ProcessId { get; }

    public float Volume
    {
        get => Session.SimpleAudioVolume.Volume * 100f;
        set { Session.SimpleAudioVolume.Volume = Math.Clamp(value / 100f, 0f, 1f); Changed(); }
    }

    public bool IsMuted
    {
        get => Session.SimpleAudioVolume.Mute;
        set { Session.SimpleAudioVolume.Mute = value; Changed(); }
    }

    internal AppAudioSession(AudioSessionControl session, string key, string appName, string deviceName, int processId)
    {
        Session = session;
        Key = key;
        AppName = appName;
        DeviceName = deviceName;
        ProcessId = processId;
    }
}
