namespace dn32.grpc.easy.client.model;

public class GrpcRetryPolicy
{
    public int RetryPolicyMaxAttempts { get; set; } = 5;

    public int RetryPolicyInitialBackoffEmMilisegundos { get; set; } = 1000;

    public int RetryPolicyMaxBackoffEmMilisegundos { get; set; } = 2000;

    public int RetryPolicyBackoffMultiplierMultiplicadoPor10 { get; set; } = 15;
}