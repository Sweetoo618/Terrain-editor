using UnityEngine;
using System;

public static class TerrainEvents
{
    public static event Action<Vector3, float, float> OnBrushApplied;
    public static event Action<TerrainEditor.EditMode> OnEditModeChanged;
    public static event Action<float> OnBrushRadiusChanged;
    public static event Action<float, TerrainEditor.EditMode> OnBrushStrengthChanged;
    public static event Action<float> OnRoughenAmountChanged;

    public static event Action OnTerrainGenerated;
    public static event Action OnTerrainRegenerated;
    public static event Action OnTerrainCleared;

    public static event Action<string> OnExportSuccess;
    public static event Action<string> OnExportFailed;

    public static event Action<float> OnCameraMoveSpeedChanged;
    public static event Action<float> OnCameraRotationSpeedChanged;
    public static event Action<float> OnCameraVerticalSpeedChanged;

    public static event Action OnSettingsReset;

    public static void InvokeBrushApplied(Vector3 position, float radius, float strength)
    {
        OnBrushApplied?.Invoke(position, radius, strength);
    }

    public static void InvokeEditModeChanged(TerrainEditor.EditMode mode)
    {
        OnEditModeChanged?.Invoke(mode);
    }

    public static void InvokeBrushRadiusChanged(float radius)
    {
        OnBrushRadiusChanged?.Invoke(radius); }

    public static void InvokeBrushStrengthChanged(float strength, TerrainEditor.EditMode mode)
    {
        OnBrushStrengthChanged?.Invoke(strength, mode);
    }

    public static void InvokeRoughenAmountChanged(float amount)
    {
        OnRoughenAmountChanged?.Invoke(amount);
    }

    public static void InvokeTerrainGenerated()
    {
        OnTerrainGenerated?.Invoke();
    }

    public static void InvokeTerrainRegenerated()
    {
        OnTerrainRegenerated?.Invoke();
    }

    public static void InvokeTerrainCleared()
    {
        OnTerrainCleared?.Invoke();
    }

    public static void InvokeExportSuccess(string message)
    {
        OnExportSuccess?.Invoke(message);
    }

    public static void InvokeExportFailed(string message)
    {
        OnExportFailed?.Invoke(message);
    }

    public static void InvokeCameraMoveSpeedChanged(float speed)
    {
        OnCameraMoveSpeedChanged?.Invoke(speed);
    }

    public static void InvokeCameraRotationSpeedChanged(float speed)
    {
        OnCameraRotationSpeedChanged?.Invoke(speed);
    }

    public static void InvokeCameraVerticalSpeedChanged(float speed)
    {
        OnCameraVerticalSpeedChanged?.Invoke(speed);
    }

    public static void InvokeSettingsReset()
    {
        OnSettingsReset?.Invoke();
    }

    public static void ClearAllEvents()
    {
        OnBrushApplied = null;
        OnEditModeChanged = null;
        OnBrushRadiusChanged = null;
        OnBrushStrengthChanged = null;
        OnRoughenAmountChanged = null;
        OnTerrainGenerated = null;
        OnTerrainRegenerated = null;
        OnTerrainCleared = null;
        OnExportSuccess = null;
        OnExportFailed = null;
        OnCameraMoveSpeedChanged = null;
        OnCameraRotationSpeedChanged = null;
        OnCameraVerticalSpeedChanged = null;
        OnSettingsReset = null;
    }
}