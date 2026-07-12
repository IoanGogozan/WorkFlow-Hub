namespace NorvixHub.Contracts.LiveDemo;

public sealed record LiveDemoCapabilitiesResponse(
    bool Enabled,
    bool BrregLiveEnabled,
    bool SharePointEnabled,
    bool ErpReceiverEnabled,
    bool FailureDemoEnabled);
