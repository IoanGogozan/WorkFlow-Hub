namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoCapabilitiesResponse(
    bool Enabled,
    bool BrregLiveEnabled,
    bool SharePointSimulatorEnabled,
    bool ErpReceiverEnabled,
    bool FailureDemoEnabled);
