using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bien4read
{
    class CropPoint
    {
        private byte[] m_pixels;
        public System.Windows.Point m_ptUp, m_ptDown, m_ptLeft, m_ptRight;
        private int wholeWidth;
        public int m_cx, m_cy, m_cw, m_ch;
        private int wholeHeight;

        public CropPoint(byte[] pixels, int startX, int startY, int endX, int endY, int depth, int searchValue, int wholeWidth, int wholeHeight, int marginMin)
        {
            this.m_pixels = pixels;
            this.wholeWidth = wholeWidth;
            this.wholeHeight = wholeHeight;

            m_ptUp = m_ptLeft = new System.Windows.Point(0, 0);
            m_ptDown = m_ptRight = new System.Windows.Point(endX, endY);

            Boolean bBlack = false;

            startX = startX + marginMin;
            endX = endX - marginMin;

            startY = startY + marginMin;
            endY = endY - marginMin;

            int startWhite = 3;

            // 위 여백 찾기
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    bBlack = isBlack(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);
                    if (bBlack)
                    {
                        if ( y < marginMin) continue;
                        
                        for (int i = startWhite; i < marginMin; i++) {
                            bool bWhite = isWhite(pixels, x, y - i, wholeWidth, wholeHeight, depth, searchValue);

                            if (bWhite == false) {
                                bBlack = false;
                                break;
                            }
                        }

                        if (bBlack) {
                            m_ptUp = new System.Windows.Point(x, y);
                            break;
                        }
                    }
                }
                if (bBlack) break;
            }

            // 아래 여백 찾기
            for (int y = endY - 1; y >= startY; y--)
            {
                for (int x = startX; x < endX; x++)
                {
                    bBlack = isBlack(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);
                    if (bBlack)
                    {
                        if (wholeHeight - y < marginMin) continue;

                        for (int i = startWhite; i < marginMin; i++)
                        {
                            bool bWhite = isWhite(pixels, x, y + i, wholeWidth, wholeHeight, depth, searchValue);

                            if (bWhite == false)
                            {
                                bBlack = false;
                                break;
                            }
                        }

                        if (bBlack)
                        {
                            m_ptDown = new System.Windows.Point(x, y);
                            break;
                        }
                    }
                }
                if (bBlack) break;
            }

            // 왼쪽 여백 찾기
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    bBlack = isBlack(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);
                    if (bBlack)
                    {
                        if (x < marginMin) continue;

                        for (int i = startWhite; i < marginMin; i++)
                        {
                            bool bWhite = isWhite(pixels, x - i, y, wholeWidth, wholeHeight, depth, searchValue);

                            if (bWhite == false)
                            {
                                bBlack = false;
                                break;
                            }
                        }

                        if (bBlack)
                        {
                            m_ptLeft = new System.Windows.Point(x, y);
                            break;
                        }
                    }
                }
                if (bBlack) break;
            }

            // 오른쪽 여백 찾기
            for (int x = endX - 1; x >= startX; x--)
            {
                for (int y = startY; y < endY; y++)
                {
                    bBlack = isBlack(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);
                    if (bBlack)
                    {
                        if (wholeWidth - x < marginMin) continue;

                        for (int i = startWhite; i < marginMin; i++)
                        {
                            bool bWhite = isWhite(pixels, x + i, y, wholeWidth, wholeHeight, depth, searchValue);

                            if (bWhite == false)
                            {
                                bBlack = false;
                                break;
                            }
                        }

                        if (bBlack)
                        {
                            m_ptRight = new System.Windows.Point(x, y);
                            break;
                        }
                    }
                }
                if (bBlack) break;
            }
        }

        private bool isWhite(byte[] pixels, int x, int y, int wholeWidth, int wholeHeight, int depth, int searchValue)
        {
            int avg = getColor(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);

            if (avg > 200)
                return true;

            return false;
        }

        // 참고: http://www.codeproject.com/Tips/240428/Work-with-bitmap-faster-with-Csharp
        private bool isBlack(byte[] pixels, int x, int y, int width, int height, int depth, int searchValue)
        {
            int avg = getColor(pixels, x, y, wholeWidth, wholeHeight, depth, searchValue);

            if (avg < searchValue)
                return true;

            return false;
        }

        private int getColor(byte[] pixels, int x, int y, int width, int height, int depth, int searchValue)
        {
            // Get color components count
            int cCount = depth / 8;

            // Get start index of the specified pixel
            int i = ((y * width) + x) * cCount;

            if (i > pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                return (r + b + g) / 3;
            }
            if (depth == 24)
            { // For 24 bpp get Red, Green and Blue
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];

                return (r + b + g) / 3;
            }
            if (depth == 8) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte c = pixels[i];

                return c;
            }

            return 0;
        }



        internal void addMargin(int marginUp, int marginLR, int marginDown)
        {
            m_cx = between((int)m_ptLeft.X - marginLR, 0, wholeWidth);
            m_cy = between((int)m_ptUp.Y - marginUp, 0, wholeHeight);
            m_cw = between((int)m_ptRight.X - m_cx + marginLR, 0, wholeWidth - m_cx);
            m_ch = between((int)m_ptDown.Y - m_cy + marginDown, 0, wholeHeight - m_cy);
        }

        private int between(int t, int lower, int upper)
        {
            if (t < lower)
                return lower;
            else if (t > upper)
                return upper;
            else
                return t;
        }

        internal System.Drawing.Rectangle getRectangle()
        {
            return new System.Drawing.Rectangle(m_cx, m_cy, m_cw, m_ch);
        }
    }
}
