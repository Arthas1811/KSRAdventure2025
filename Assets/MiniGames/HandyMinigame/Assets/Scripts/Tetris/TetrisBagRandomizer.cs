using System;
using System.Collections.Generic;

//draws tetris pieces
public class TetrisBagRandomizer
{
    private static readonly TetrisTetromino[] AllTetrominoes =
    {
        TetrisTetromino.I,
        TetrisTetromino.O,
        TetrisTetromino.T,
        TetrisTetromino.S,
        TetrisTetromino.Z,
        TetrisTetromino.J,
        TetrisTetromino.L,
    };

    private readonly Random random;
    private readonly Queue<TetrisTetromino> queue = new Queue<TetrisTetromino>();

    //sets tetris bag
    public TetrisBagRandomizer()
        : this(Environment.TickCount)
    {
    }

    //sets tetris bag
    public TetrisBagRandomizer(int seed)
    {
        random = new Random(seed);
    }

    public int RemainingInQueue => queue.Count;

    //draws tetris type
    public TetrisTetromino Draw()
    {
        if (queue.Count == 0)
        {
            GenerateBag();
        }

        return queue.Dequeue();
    }

    //checks next tetris type
    public TetrisTetromino PeekNext()
    {
        if (queue.Count == 0)
        {
            GenerateBag();
        }

        return queue.Peek();
    }

    //makes tetris bag
    private void GenerateBag()
    {
        List<TetrisTetromino> bag = new List<TetrisTetromino>(AllTetrominoes);

        for (int i = bag.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            TetrisTetromino current = bag[i];
            bag[i] = bag[swapIndex];
            bag[swapIndex] = current;
        }

        foreach (TetrisTetromino type in bag)
        {
            queue.Enqueue(type);
        }
    }
}
