using System.Runtime.InteropServices;

namespace Tonome.Compositor.Wayland;

public class WaylandServer : IDisposable
{
    private IntPtr _display;
    private IntPtr _eventLoop;
    private bool _running;

    public IntPtr Display => _display;
    public string SocketPath { get; private set; } = "";

    public WaylandServer()
    {
        Initialize();
    }

    private void Initialize()
    {
        _display = NativeMethods.wl_display_create();
        if (_display == IntPtr.Zero)
            throw new Exception("Failed to create Wayland display");

        _eventLoop = NativeMethods.wl_display_get_event_loop(_display);
    }

    public void Start()
    {
        _running = true;
        var socketPtr = NativeMethods.wl_display_add_socket_auto(_display);
        SocketPath = socketPtr != IntPtr.Zero ? Marshal.PtrToStringAnsi(socketPtr) ?? "" : "";

        if (string.IsNullOrEmpty(SocketPath))
            throw new Exception("Failed to add Wayland socket");

        NativeMethods.wl_global_create(
            _display,
            IntPtr.Zero,
            4,
            IntPtr.Zero,
            OnBindCompositor);

        NativeMethods.wl_global_create(
            _display,
            IntPtr.Zero,
            1,
            IntPtr.Zero,
            OnBindShell);

        NativeMethods.wl_global_create(
            _display,
            IntPtr.Zero,
            7,
            IntPtr.Zero,
            OnBindSeat);

        NativeMethods.wl_global_create(
            _display,
            IntPtr.Zero,
            4,
            IntPtr.Zero,
            OnBindOutput);
    }

    public void RunEventLoop()
    {
        while (_running)
        {
            NativeMethods.wl_event_loop_dispatch(_eventLoop, 0);
        }
    }

    public void Stop()
    {
        _running = false;
    }

    private static void OnBindCompositor(IntPtr client, IntPtr data, uint version, uint id) { }
    private static void OnBindShell(IntPtr client, IntPtr data, uint version, uint id) { }
    private static void OnBindSeat(IntPtr client, IntPtr data, uint version, uint id) { }
    private static void OnBindOutput(IntPtr client, IntPtr data, uint version, uint id) { }

    public void Dispose()
    {
        if (_display != IntPtr.Zero)
        {
            NativeMethods.wl_display_destroy(_display);
            _display = IntPtr.Zero;
        }
    }
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WlGlobalBindFunc(IntPtr client, IntPtr data, uint version, uint id);

internal static class NativeMethods
{
    private const string LibWayland = "libwayland-server.so.0";

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_display_create();

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern void wl_display_destroy(IntPtr display);

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_display_get_event_loop(IntPtr display);

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_display_add_socket_auto(IntPtr display);

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_global_create(
        IntPtr display,
        IntPtr interfacePtr,
        uint version,
        IntPtr data,
        WlGlobalBindFunc bindFunc);

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wl_event_loop_dispatch(IntPtr loop, int timeout);

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_compositor_interface_get();

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_shell_interface_get();

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_seat_interface_get();

    [DllImport(LibWayland, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wl_output_interface_get();
}
