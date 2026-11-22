using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class JSLMotionInput
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MotionState
    {
        public float quatW, quatX, quatY, quatZ;
        public float accelX, accelY, accelZ;
        public float gravX, gravY, gravZ;
    }

    private const int DualShock4 = 4;
    private const int DualSense = 5;


    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern int JslConnectDevices();

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern int JslGetConnectedDeviceHandles([Out] int[] handles, int size);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern int JslGetControllerType(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool JslStillConnected(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern void JslSetAutomaticCalibration(int deviceId, [MarshalAs(UnmanagedType.I1)] bool enabled);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern MotionState JslGetMotionState(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern void JslDisconnectAndDisposeAll();


    private static int deviceId = -1;
    private static int lastFrame = -1;
    private static float nextScanTime;

    private static bool libraryAvailable = true;
    private static bool available;
    private static Quaternion referenceRotation = Quaternion.identity;
    private static bool referenceReady;
    private static Vector3 angles;

    private static bool loggedNoSupportedController;


    public static bool TryGetAngles(out Vector3 result)
    {
        Update();
        result = angles;
        return available;
    }

    private static void Update()
    {
        if (lastFrame == Time.frameCount)
            return;
        
        lastFrame = Time.frameCount;
        if (!libraryAvailable)
            return;

        try
        {
            if (deviceId >= 0 && !JslStillConnected(deviceId))
            {
                deviceId = -1;
                referenceReady = false;
                angles = Vector3.zero;
                available = false;
            }
            if (deviceId < 0 && Time.unscaledTime >= nextScanTime)
            {
                nextScanTime = Time.unscaledTime + 1.0f;
                Connect();
            }
            if (deviceId < 0)
            {
                available = false;
                angles = Vector3.zero;
                return;
            }

            MotionState state = JslGetMotionState(deviceId);
            Quaternion rotation = new(state.quatX, state.quatY, state.quatZ, state.quatW);
            if (Quaternion.Dot(rotation, rotation) < 1.0e-6f)
            {
                available = false;
                return;
            }

            rotation = rotation.normalized;
            if (!referenceReady)
            {
                Vector3 gravity = new(state.gravX, state.gravY, state.gravZ);
                if (gravity.sqrMagnitude < 1.0e-6f)
                {
                    angles = Vector3.zero;
                    available = false;
                    return;
                }

                referenceRotation = rotation;
                referenceReady = true;
                angles = Vector3.zero;
                available = true;
                return;
            }

            Quaternion relativeRotation = Quaternion.Inverse(referenceRotation) * rotation;
            Matrix4x4 m = Matrix4x4.Rotate(relativeRotation);

            float pitch = Mathf.Asin(Mathf.Clamp(m.m21, -1.0f, 1.0f)) * Mathf.Rad2Deg;
            float yaw = Mathf.Atan2(-m.m20, m.m22) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(-m.m01, m.m11) * Mathf.Rad2Deg;

            angles = new(
                Mathf.DeltaAngle(0.0f, -yaw) / 180.0f,
                Mathf.DeltaAngle(0.0f, -pitch) / 180.0f,
                Mathf.DeltaAngle(0.0f, roll) / 180.0f
            );

            available = true;
        }
        catch (DllNotFoundException)
        {
            DisableLibrary();
        }
        catch (EntryPointNotFoundException)
        {
            DisableLibrary();
        }
        catch (BadImageFormatException)
        {
            DisableLibrary();
        }
    }

    private static void Connect()
    {
        int newlyConnected = JslConnectDevices();

        int[] handles = new int[16];
        int count = Mathf.Clamp(JslGetConnectedDeviceHandles(handles, handles.Length), 0, handles.Length);
        int dualSenseFallback = -1;
        for (int i = 0; i < count; i++)
        {
            int handle = handles[i];
            int type = JslGetControllerType(handle);
            if (!loggedNoSupportedController)
                Debug.Log($"JSL: handle={handle}, type={type}");
            if (type == DualShock4)
            {
                deviceId = handle;
                break;
            }
            if (type == DualSense && dualSenseFallback < 0)
                dualSenseFallback = handle;
        }
        if (deviceId < 0)
            deviceId = dualSenseFallback;
        if (deviceId < 0)
        {
            if (!loggedNoSupportedController)
            {
                Debug.LogWarning($"JSL did not select a supported DualShock 4/DualSense. New connections={newlyConnected}; handles found={count}.");
                loggedNoSupportedController = true;
            }
            return;
        }

        JslSetAutomaticCalibration(deviceId, true);
        referenceReady = false;
        angles = Vector3.zero;
        available = false;
    }

    private static void ResetCalibration()
    {
        referenceRotation = Quaternion.identity;
        referenceReady = false;
        angles = Vector3.zero;
        available = false;
    }

    private static void DisableLibrary()
    {
        libraryAvailable = false;
        available = false;
        Debug.LogError("JSL motion input unavailable. Chech the JoyShockLibrary DLL import settings.");
    }

    private static void Shutdown()
    {
        if (!libraryAvailable)
            return;
        try
        {
            JslDisconnectAndDisposeAll();
            deviceId = -1;
        }
        catch (DllNotFoundException) {}
        catch (EntryPointNotFoundException) {}
    }
}
