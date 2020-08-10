namespace Baku.VMagicMirrorConfig
{
    static class ReceiveMessageNames
    {
        public const string RequestShowError = nameof(RequestShowError);
        public const string SetUnityProcessId = nameof(SetUnityProcessId);
        public const string CloseConfigWindow = nameof(CloseConfigWindow);
        public const string SetCalibrationFaceData = nameof(SetCalibrationFaceData);
        public const string SetBlendShapeNames = nameof(SetBlendShapeNames);
        public const string AutoAdjustResults = nameof(AutoAdjustResults);
        public const string AutoAdjustEyebrowResults = nameof(AutoAdjustEyebrowResults);
        public const string UpdateDeviceLayout = nameof(UpdateDeviceLayout);

        public const string ExtraBlendShapeClipNames = nameof(ExtraBlendShapeClipNames);

        public const string MidiNoteOn = nameof(MidiNoteOn);

        public const string ExTrackerCalibrateComplete = nameof(ExTrackerCalibrateComplete);
        public const string ExTrackerSetPerfectSyncMissedClipNames = nameof(ExTrackerSetPerfectSyncMissedClipNames);

        public const string VRoidModelLoadCompleted = nameof(VRoidModelLoadCompleted);
        public const string VRoidModelLoadCanceled = nameof(VRoidModelLoadCanceled);
    }
}
