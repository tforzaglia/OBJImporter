using UnityEngine;
using System.IO;

public class LineReader
{
    public char[] word;
    public int wordSize;
    public bool endReached;

    private StreamReader reader;
    private int bufferSize;
    private char[] buffer;

    public char currentChar;
    private int currentPosition = 0;
    private int maxPosition = 0;

    public LineReader(StreamReader reader, int bufferSize)
    {
        this.reader = reader;
        this.bufferSize = bufferSize;

        buffer = new char[bufferSize];
        word = new char[bufferSize];

        MoveNext();
    }

    public void MoveNext()
    {
        currentPosition++;
        if (currentPosition >= maxPosition)
        {
            if (reader.EndOfStream == true)
            {
                currentChar = char.MinValue;
                endReached = true;
                return;
            }

            currentPosition = 0;
            maxPosition = reader.Read(buffer, 0, bufferSize);
        }
        currentChar = buffer[currentPosition];
    }

    public void SkipWhitespaces()
    {
        while (char.IsWhiteSpace(currentChar))
        {
            MoveNext();
        }
    }

    public void SkipWhitespaces(out bool newLinePassed)
    {
        newLinePassed = false;
        while (char.IsWhiteSpace(currentChar))
        {
            if (currentChar == '\r' || currentChar == '\n')
            {
                newLinePassed = true;
            }
            MoveNext();
        }
    }

    public void SkipUntilNewLine()
    {
        while (currentChar != char.MinValue && currentChar != '\n' && currentChar != '\r')
        {
            MoveNext();
        }
        SkipNewLineSymbols();
    }

    public void ReadUntilWhiteSpace()
    {
        wordSize = 0;
        while (currentChar != char.MinValue && char.IsWhiteSpace(currentChar) == false)
        {
            word[wordSize] = currentChar;
            wordSize++;
            MoveNext();
        }
    }

    public bool Is(string other)
    {
        if (other.Length != this.wordSize)
        {
            return false;
        }

        for (int i = 0; i < wordSize; i++)
        {
            if (word[i] != other[i])
            {
                return false;
            }
        }

        return true;
    }

    public Vector3 ReadVector()
    {
        SkipWhitespaces();
        float x = ReadFloat();
        SkipWhitespaces();
        float y = ReadFloat();
        SkipWhitespaces(out var newLinePassed);
        float z = 0f;
        if (newLinePassed == false)
        {
            z = ReadFloat();
        }
        return new Vector3(x, y, z);
    }

    public int ReadInt()
    {
        int result = 0;
        bool isNegative = currentChar == '-';
        if (isNegative == true)
        {
            MoveNext();
        }

        while (currentChar >= '0' && currentChar <= '9')
        {
            var digit = currentChar - '0';
            result = result * 10 + digit;
            MoveNext();
        }

        return (isNegative == true) ? -result : result;
    }

    private void SkipNewLineSymbols()
    {
        while (currentChar == '\n' || currentChar == '\r')
        {
            MoveNext();
        }
    }

    private float ReadFloat()
    {
        bool isNegative = currentChar == '-';
        if (isNegative)
        {
            MoveNext();
        }

        var num = (float)ReadInt();
        if (currentChar == '.' || currentChar == ',')
        {
            MoveNext();
            num += ReadFloatEnd();

            if (currentChar == 'e' || currentChar == 'E')
            {
                MoveNext();
                var exp = ReadInt();
                num *= Mathf.Pow(10f, exp);
            }
        }
        if (isNegative == true)
        {
            num = -num;
        }

        return num;
    }

    private float ReadFloatEnd()
    {
        float result = 0f;

        var exp = 0.1f;
        while (currentChar >= '0' && currentChar <= '9')
        {
            var digit = currentChar - '0';
            result += digit * exp;

            exp *= 0.1f;

            MoveNext();
        }

        return result;
    }
}
