namespace AlgorithmVisualizer
{
    public interface IAlgorithmVisualizer
    {
        bool NextStep();
        void Reset();
        int Steps { get; }

        // Performance metrics properties
        long NanosecondsTaken { get; }
        long CpuCyclesTaken { get; }
        long MemoryUsed { get; }
    }
}