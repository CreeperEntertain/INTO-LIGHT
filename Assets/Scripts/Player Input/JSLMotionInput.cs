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
    [StructLayout(LayoutKind.Sequential)]
    private struct IMUState
    {
        public float accelX, accelY, accelZ;
        public float gyroX, gyroY, gyroZ;
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
    private static extern IMUState JslGetIMUState(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    private static extern void JslDisconnectAndDisposeAll();


    private static int deviceId = -1;
    private static int lastFrame = -1;
    private static float nextScanTime;

    private static bool libraryAvailable = true;
    private static bool available;
    private static Quaternion referenceRotation = Quaternion.identity;
    private static Vector3 angles;

    private static bool loggedNoSupportedController;
    private static float nextAngleLogTime;


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
            IMUState imu = JslGetIMUState(deviceId);
            Quaternion rotation = new(state.quatX, state.quatY, state.quatZ, state.quatW);
            if (Quaternion.Dot(rotation, rotation) < 1.0e-6f)
            {
                available = false;
                return;
            }

            rotation = rotation.normalized;
            Vector3 relativeAngles = (Quaternion.Inverse(referenceRotation) * rotation).eulerAngles;
            angles = new(
                Mathf.DeltaAngle(0.0f, relativeAngles.x),
                Mathf.DeltaAngle(0.0f, relativeAngles.y),
                Mathf.DeltaAngle(0.0f, relativeAngles.z)
            );
            if (Time.unscaledTime >= nextAngleLogTime)
            {
                nextAngleLogTime = Time.unscaledTime + 0.5f;
                Debug.Log(
                    $"JSL quaternion: W={state.quatW:F3}, X={state.quatX:F3}, Y={state.quatY:F3}, Z={state.quatZ:F3} | " +
                    $"Gravity: X={state.gravX:F3}, Y={state.gravY:F3}, Z={state.gravZ:F3} | " +
                    $"Gyro: X={imu.gyroX:F2}, Y={imu.gyroY:F2}, Z={imu.gyroZ:F2} | " +
                    $"Accel: X={imu.accelX:F2}, Y={imu.accelY:F2}, Z={imu.accelZ:F2} | " +
                    $"Relative: {angles}"
                );
            }
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

        loggedNoSupportedController = false;
        Debug.Log($"JSL selected controller handle={deviceId}.");

        JslSetAutomaticCalibration(deviceId, true);
        MotionState state = JslGetMotionState(deviceId);
        referenceRotation = new(state.quatX, state.quatY, state.quatZ, state.quatW);
        if (Quaternion.Dot(referenceRotation, referenceRotation) < 1.0e-6f)
            referenceRotation = Quaternion.identity;
        else
            referenceRotation = referenceRotation.normalized;
        angles = Vector3.zero;
        available = true;
    }

    private static void ResetCalibration()
    {
        referenceRotation = Quaternion.identity;
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
