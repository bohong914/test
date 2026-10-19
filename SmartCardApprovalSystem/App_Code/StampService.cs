using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class StampService
{
    /// <summary>
    /// 動態篆刻朱紅職官章 (支援 Square 傳統方章、Round 圓形職官章、Custom 自訂透明印鑑)
    /// </summary>
    public static void GenerateStampPng(
        string outputPngPath,
        string shape,
        string topTitle,
        string centerText,
        string customStampPhysicalPath,
        DateTime stampDate)
    {
        string dir = Path.GetDirectoryName(outputPngPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        int width = 260;
        int height = 260;

        using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            // 朱紅印泥色澤 (略帶透明感)
            Color vermilion = Color.FromArgb(228, 210, 35, 35);

            if (string.Equals(shape, "Custom", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(customStampPhysicalPath) &&
                File.Exists(customStampPhysicalPath))
            {
                using (Image customImg = Image.FromFile(customStampPhysicalPath))
                {
                    g.DrawImage(customImg, new Rectangle(20, 15, 220, 195));
                }
            }
            else if (string.Equals(shape, "Round", StringComparison.OrdinalIgnoreCase))
            {
                // 繪製圓形職官章 (雙層同心朱紅圓框)
                using (Pen outerPen = new Pen(vermilion, 7f))
                using (Pen innerPen = new Pen(vermilion, 2.5f))
                {
                    g.DrawEllipse(outerPen, 16, 16, 228, 228);
                    g.DrawEllipse(innerPen, 26, 26, 208, 208);
                }

                DrawStampTexts(g, vermilion, topTitle, centerText, stampDate, true);
            }
            else
            {
                // 繪製傳統方章 (外粗內細雙框)
                using (Pen outerPen = new Pen(vermilion, 8f))
                using (Pen innerPen = new Pen(vermilion, 2.5f))
                {
                    g.DrawRectangle(outerPen, 18, 18, 224, 224);
                    g.DrawRectangle(innerPen, 28, 28, 204, 204);
                }

                DrawStampTexts(g, vermilion, topTitle, centerText, stampDate, false);
            }

            bmp.Save(outputPngPath, ImageFormat.Png);
        }
    }

    private static void DrawStampTexts(Graphics g, Color vermilion, string topTitle, string centerText, DateTime stampDate, bool isRound)
    {
        if (string.IsNullOrWhiteSpace(topTitle)) topTitle = "智慧卡簽核中心";
        if (string.IsNullOrWhiteSpace(centerText)) centerText = "審核官職章";

        using (SolidBrush brush = new SolidBrush(vermilion))
        using (StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            // 1. 頂部頭銜
            using (Font topFont = new Font("標楷體", 18, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                g.DrawString(topTitle, topFont, brush, new RectangleF(24, isRound ? 46 : 38, 212, 36), sfCenter);
            }

            // 分隔細線
            using (Pen linePen = new Pen(vermilion, 2f))
            {
                g.DrawLine(linePen, 42, 82, 218, 82);
                g.DrawLine(linePen, 42, 182, 218, 182);
            }

            // 2. 中央主印文 (篆刻粗體視覺)
            using (Font centerFont = new Font("標楷體", 27, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                g.DrawString(centerText, centerFont, brush, new RectangleF(24, 86, 212, 92), sfCenter);
            }

            // 3. 底部落章日期戳記
            string dateStr = "核章 " + stampDate.ToString("yyyy.MM.dd");
            using (Font dateFont = new Font("Microsoft JhengHei", 14, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                g.DrawString(dateStr, dateFont, brush, new RectangleF(24, 186, 212, 34), sfCenter);
            }
        }
    }

    /// <summary>
    /// 若申請人測試時未準備照片，可自動產生預設證件照
    /// </summary>
    public static void EnsureDefaultAvatarExists(string avatarPhysicalPath, string fullName)
    {
        if (File.Exists(avatarPhysicalPath)) return;
        string dir = Path.GetDirectoryName(avatarPhysicalPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using (Bitmap bmp = new Bitmap(300, 380))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(226, 232, 240));
            using (SolidBrush headBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            {
                g.FillEllipse(headBrush, 95, 70, 110, 110);
                g.FillPie(headBrush, 40, 195, 220, 220, 180, 180);
            }
            using (Font f = new Font("Microsoft JhengHei", 18, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                g.DrawString(fullName + " 證件照", f, textBrush, new RectangleF(0, 330, 300, 40), sf);
            }
            bmp.Save(avatarPhysicalPath, ImageFormat.Jpeg);
        }
    }
}
