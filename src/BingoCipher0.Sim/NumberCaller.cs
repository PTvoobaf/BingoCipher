namespace BingoCipher0.Sim;

public sealed class NumberCaller
{
    private readonly List<int> drawOrder;
    private readonly List<int> calledNumbers = [];
    private int nextIndex;

    public NumberCaller(int minimum, int maximum, Random random)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimum);
        if (maximum < minimum) throw new ArgumentOutOfRangeException(nameof(maximum));
        ArgumentNullException.ThrowIfNull(random);

        drawOrder = Enumerable.Range(minimum, maximum - minimum + 1).ToList();
        for (var index = drawOrder.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (drawOrder[index], drawOrder[swapIndex]) = (drawOrder[swapIndex], drawOrder[index]);
        }
    }

    public IReadOnlyList<int> DrawOrder => drawOrder.AsReadOnly();
    public IReadOnlyList<int> CalledNumbers => calledNumbers.AsReadOnly();
    public bool IsExhausted => nextIndex == drawOrder.Count;

    public int DrawNext()
    {
        if (IsExhausted) throw new InvalidOperationException("The number caller has no numbers remaining.");
        var number = drawOrder[nextIndex++];
        calledNumbers.Add(number);
        return number;
    }
}
