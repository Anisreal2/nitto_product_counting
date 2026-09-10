using BeevisionSolution.Models;
using System;

namespace BeevisionSolution.Utils
{
    public static class LayoutGenerator
    {
        public static int[,] GenerateMatrixLayout(LayoutSetting setting)
        {
            int rows = setting.RowCount;
            int cols = setting.ColumnCount;
            int[,] matrix = new int[rows, cols];
            int current = 1;
            int totalItems = Math.Min(setting.TotalItem, rows * cols);

            if (!setting.IsColumnBased)
            {
                for (int r = 0; r < rows; r++)
                {
                    bool reverse = false;
                    switch (setting.DirectionType)
                    {
                        case DirectionType.Row_LeftToRight_OddRow: reverse = r % 2 == 1; break;
                        case DirectionType.Row_LeftToRight_AllRow: reverse = false; break;
                        case DirectionType.Row_RightToLeft_OddRow: reverse = r % 2 == 0; break;
                        case DirectionType.Row_RightToLeft_AllRow: reverse = true; break;
                    }

                    if (!reverse)
                    {
                        for (int c = 0; c < cols && current <= totalItems; c++)
                            matrix[r, c] = current++;
                    }
                    else
                    {
                        for (int c = cols - 1; c >= 0 && current <= totalItems; c--)
                            matrix[r, c] = current++;
                    }
                }
            }
            else
            {
                bool isNewZigZag = IsNewZigZagType(setting.DirectionType);

                if (isNewZigZag)
                {
                    bool rightToLeft = setting.DirectionType == DirectionType.Column_ZigZag_RightToLeft_TopDownOdd ||
                                       setting.DirectionType == DirectionType.Column_ZigZag_RightToLeft_BottomUpOdd;

                    int startCol = rightToLeft ? cols - 1 : 0;
                    int endCol = rightToLeft ? -1 : cols;
                    int colStep = rightToLeft ? -1 : 1;

                    for (int c = startCol; c != endCol; c += colStep)
                    {
                        bool upward = IsUpwardDirection(setting.DirectionType, c, cols, rightToLeft);

                        if (!upward)
                        {
                            // Từ trên xuống
                            for (int r = 0; r < rows && current <= totalItems; r++)
                                matrix[r, c] = current++;
                        }
                        else
                        {
                            // Từ dưới lên
                            for (int r = rows - 1; r >= 0 && current <= totalItems; r--)
                                matrix[r, c] = current++;
                        }
                    }
                }
                else
                {
                    for (int c = 0; c < cols; c++)
                    {
                        bool reverse = false;
                        switch (setting.DirectionType)
                        {
                            case DirectionType.Column_TopToBottom_OddCol: reverse = c % 2 == 1; break;
                            case DirectionType.Column_TopToBottom_AllCol: reverse = false; break;
                            case DirectionType.Column_BottomToTop_OddCol: reverse = c % 2 == 0; break;
                            case DirectionType.Column_BottomToTop_AllCol: reverse = true; break;
                        }

                        if (!reverse)
                        {
                            for (int r = 0; r < rows && current <= totalItems; r++)
                                matrix[r, c] = current++;
                        }
                        else
                        {
                            for (int r = rows - 1; r >= 0 && current <= totalItems; r--)
                                matrix[r, c] = current++;
                        }
                    }
                }
            }

            return matrix;
        }

        private static bool IsNewZigZagType(DirectionType type)
        {
            return type == DirectionType.Column_ZigZag_RightToLeft_TopDownOdd ||
                   type == DirectionType.Column_ZigZag_RightToLeft_BottomUpOdd;
        }

        private static bool IsUpwardDirection(DirectionType type, int currentColIndex, int totalCols, bool rightToLeft)
        {
            int logicalCol = rightToLeft ? (totalCols - 1 - currentColIndex) : currentColIndex;

            bool oddColIsUp = type == DirectionType.Column_ZigZag_RightToLeft_BottomUpOdd;

            return oddColIsUp ? (logicalCol % 2 == 1) : (logicalCol % 2 == 0);
        }
    }
}