namespace dn32.grpc.easy.client.model;

public class GrpcRetryPolicy
{
    public int RetryPolicyMaxAttempts { get; set; } = 5;

    public int RetryPolicyInitialBackoff { get; set; } = 1000;

    public int RetryPolicyMaxBackoff { get; set; } = 2000;

    public int RetryPolicyBackoffMultiplier { get; set; } = 15;
}