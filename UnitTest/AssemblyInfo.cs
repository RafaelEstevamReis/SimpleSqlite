using Xunit;

// Simple.Sqlite exposes global mutable static configuration
// (e.g. ConnectionFactory.HandleGuidAsByteArray). Tests that toggle it would
// race under xUnit's default per-class parallelism, so the suite runs serially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
