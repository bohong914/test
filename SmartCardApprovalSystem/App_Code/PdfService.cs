using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

public static class PdfService
{
    private static readonly string[] AllReasons = new string[]
    {
        "新進人員", "卡片遺失", "卡片毀損", "職務調動", "權限變更", "屆期換發"
    };

    /// <summary>
    /// 純 C# 動態生成 A4 正式 PDF 申請單 (含申請人照片、勾選事由、四級職官章與配發卡號)
    /// 採用原生 PDF 1.4 DCTDecode 封裝，免安裝任何第三方 DLL，100% 支援中文與印章透明疊加
    /// </summary>
    public static void GenerateApplicationPdf(
        string outputPdfPath,
        string empNo,
        string fullName,
        string department,
        string jobTitle,
        string cardCategory,
        string applyReasons,
        string photoPhysicalPath,
        string[] stampPaths,
        string issuedCardNo,
        int appId = 0)
    {
        string folder = Path.GetDirectoryName(outputPdfPath);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        int pageW = 1240;
        int pageH = 1754;

        byte[] jpegBytes;
        using (Bitmap pageBmp = new Bitmap(pageW, pageH, PixelFormat.Format24bppRgb))
        using (Graphics g = Graphics.FromImage(pageBmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.White);

            Pen borderOuter = new Pen(Color.FromArgb(15, 23, 42), 4f);
            Pen borderInner = new Pen(Color.FromArgb(51, 65, 85), 1.8f);
            SolidBrush headerBg = new SolidBrush(Color.FromArgb(241, 245, 249));
            SolidBrush textDark = new SolidBrush(Color.FromArgb(15, 23, 42));
            SolidBrush textMuted = new SolidBrush(Color.FromArgb(71, 85, 105));

            Font titleFont = new Font("Microsoft JhengHei", 32, FontStyle.Bold, GraphicsUnit.Pixel);
            Font subTitleFont = new Font("Microsoft JhengHei", 18, FontStyle.Bold, GraphicsUnit.Pixel);
            Font labelFont = new Font("Microsoft JhengHei", 19, FontStyle.Bold, GraphicsUnit.Pixel);
            Font valueFont = new Font("Microsoft JhengHei", 20, FontStyle.Regular, GraphicsUnit.Pixel);
            Font stampHeaderFont = new Font("Microsoft JhengHei", 17, FontStyle.Bold, GraphicsUnit.Pixel);

            StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            StringFormat sfLeft = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

            // 1. A4 雙層外框與標題
            g.DrawRectangle(borderOuter, 60, 60, pageW - 120, pageH - 120);
            g.DrawRectangle(borderInner, 70, 70, pageW - 140, pageH - 140);

            g.DrawString("智慧卡申請與四級職官章簽核表單", titleFont, textDark, new RectangleF(70, 95, pageW - 140, 50), sfCenter);
            string docMeta = string.Format("表單編號：{0}  填表日期：{1:yyyy-MM-dd HH:mm}  機密等級：內部限閱",
                appId > 0 ? "#" + appId.ToString("D5") : "預覽草稿 (尚未送出)", DateTime.Now);
            g.DrawString(docMeta, subTitleFont, textMuted, new RectangleF(80, 152, pageW - 160, 30), sfCenter);

            // 2. 第一區：申請人基本資料與證件照
            int tableLeft = 95;
            int tableWidth = pageW - 190; // 1050
            int y = 200;

            g.FillRectangle(headerBg, tableLeft, y, tableWidth, 46);
            g.DrawRectangle(borderInner, tableLeft, y, tableWidth, 46);
            g.DrawString("壹、申請人基本資料與證件照", labelFont, textDark, new RectangleF(tableLeft + 15, y, 500, 46), sfLeft);
            y += 46;

            int photoBoxW = 260;
            int infoWidth = tableWidth - photoBoxW; // 790
            int rowH = 75;

            // 左側四列基本資料
            string[,] basicRows = new string[,]
            {
                { "申請人工號", empNo },
                { "申請人姓名", fullName },
                { "所屬部門",   department },
                { "目前職稱",   jobTitle }
            };

            for (int i = 0; i < 4; i++)
            {
                int rowY = y + (i * rowH);
                g.FillRectangle(headerBg, tableLeft, rowY, 200, rowH);
                g.DrawRectangle(borderInner, tableLeft, rowY, 200, rowH);
                g.DrawString(basicRows[i, 0], labelFont, textDark, new RectangleF(tableLeft + 16, rowY, 180, rowH), sfLeft);

                g.DrawRectangle(borderInner, tableLeft + 200, rowY, infoWidth - 200, rowH);
                g.DrawString(basicRows[i, 1], valueFont, textDark, new RectangleF(tableLeft + 220, rowY, infoWidth - 230, rowH), sfLeft);
            }

            // 右側證件照框 (高度 300)
            Rectangle photoCell = new Rectangle(tableLeft + infoWidth, y, photoBoxW, rowH * 4);
            g.DrawRectangle(borderInner, photoCell);
            if (!string.IsNullOrEmpty(photoPhysicalPath) && File.Exists(photoPhysicalPath))
            {
                using (Image photoImg = Image.FromFile(photoPhysicalPath))
                {
                    g.DrawImage(photoImg, new Rectangle(photoCell.X + 25, photoCell.Y + 18, photoCell.Width - 50, photoCell.Height - 36));
                }
            }
            else
            {
                g.DrawString("證件照黏貼處", labelFont, textMuted, photoCell, sfCenter);
            }

            y += rowH * 4 + 25;

            // 3. 第二區：申請卡片類別與多選事由
            g.FillRectangle(headerBg, tableLeft, y, tableWidth, 46);
            g.DrawRectangle(borderInner, tableLeft, y, tableWidth, 46);
            g.DrawString("貳、申請卡片類別與申請事由", labelFont, textDark, new RectangleF(tableLeft + 15, y, 500, 46), sfLeft);
            y += 46;

            // 申請卡片類別
            g.FillRectangle(headerBg, tableLeft, y, 200, 75);
            g.DrawRectangle(borderInner, tableLeft, y, 200, 75);
            g.DrawString("申請卡片類別", labelFont, textDark, new RectangleF(tableLeft + 16, y, 180, 75), sfLeft);

            g.DrawRectangle(borderInner, tableLeft + 200, y, tableWidth - 200, 75);
            string[] categories = new string[] { "一般通行卡", "機房特權卡", "臨時訪客卡" };
            string catLine = "";
            foreach (string c in categories)
            {
                bool checkedCat = string.Equals(cardCategory, c, StringComparison.OrdinalIgnoreCase);
                catLine += (checkedCat ? "■ " : "□ ") + c + "   ";
            }
            g.DrawString(catLine, valueFont, textDark, new RectangleF(tableLeft + 220, y, tableWidth - 240, 75), sfLeft);
            y += 75;

            // 申請事由 (多選 CheckBox)
            g.FillRectangle(headerBg, tableLeft, y, 200, 110);
            g.DrawRectangle(borderInner, tableLeft, y, 200, 110);
            g.DrawString("申請事由 (複選)", labelFont, textDark, new RectangleF(tableLeft + 16, y, 180, 110), sfLeft);

            g.DrawRectangle(borderInner, tableLeft + 200, y, tableWidth - 200, 110);
            string reasonLine1 = "";
            string reasonLine2 = "";
            for (int i = 0; i < AllReasons.Length; i++)
            {
                bool isChecked = !string.IsNullOrEmpty(applyReasons) && applyReasons.Contains(AllReasons[i]);
                string itemText = (isChecked ? "■ " : "□ ") + AllReasons[i] + "    ";
                if (i < 3) reasonLine1 += itemText;
                else reasonLine2 += itemText;
            }
            g.DrawString(reasonLine1, valueFont, textDark, new RectangleF(tableLeft + 220, y + 12, tableWidth - 240, 42), sfLeft);
            g.DrawString(reasonLine2, valueFont, textDark, new RectangleF(tableLeft + 220, y + 56, tableWidth - 240, 42), sfLeft);
            y += 110;

            // 第四級製卡中心核發實體卡號欄
            g.FillRectangle(headerBg, tableLeft, y, 200, 70);
            g.DrawRectangle(borderInner, tableLeft, y, 200, 70);
            g.DrawString("配發實體卡號", labelFont, textDark, new RectangleF(tableLeft + 16, y, 180, 70), sfLeft);

            g.DrawRectangle(borderInner, tableLeft + 200, y, tableWidth - 200, 70);
            string cardNoDisplay = string.IsNullOrWhiteSpace(issuedCardNo)
                ? "（待第四級「總務製卡中心」核定發卡時填寫）"
                : "【已核發實體卡號】 " + issuedCardNo;
            Brush cardNoBrush = string.IsNullOrWhiteSpace(issuedCardNo) ? textMuted : new SolidBrush(Color.FromArgb(5, 122, 85));
            g.DrawString(cardNoDisplay, labelFont, cardNoBrush, new RectangleF(tableLeft + 220, y, tableWidth - 240, 70), sfLeft);
            y += 95;

            // 4. 第三區：四級職官章簽核欄
            g.FillRectangle(headerBg, tableLeft, y, tableWidth, 46);
            g.DrawRectangle(borderInner, tableLeft, y, tableWidth, 46);
            g.DrawString("參、四級審核主官職官章簽核欄 (系統自動壓印職官章與日期戳記)", labelFont, textDark, new RectangleF(tableLeft + 15, y, 800, 46), sfLeft);
            y += 46;

            string[] stageTitles = new string[]
            {
                "第一級：直屬主管初審",
                "第二級：部門主管複審",
                "第三級：資安管制官審查",
                "第四級：總務製卡中心核定"
            };

            int colW = tableWidth / 4;
            int stampBoxH = 330;

            for (int i = 0; i < 4; i++)
            {
                int colX = tableLeft + (i * colW);
                int currentW = (i == 3) ? (tableWidth - colW * 3) : colW;

                // 欄位標頭
                g.FillRectangle(headerBg, colX, y, currentW, 52);
                g.DrawRectangle(borderInner, colX, y, currentW, 52);
                g.DrawString(stageTitles[i], stampHeaderFont, textDark, new RectangleF(colX, y, currentW, 52), sfCenter);

                // 落章區方格
                Rectangle stampRect = new Rectangle(colX, y + 52, currentW, stampBoxH);
                g.DrawRectangle(borderInner, stampRect);

                // 若該級已審核通過，壓印對應職官章
                if (stampPaths != null && i < stampPaths.Length && !string.IsNullOrEmpty(stampPaths[i]) && File.Exists(stampPaths[i]))
                {
                    using (Image stampImg = Image.FromFile(stampPaths[i]))
                    {
                        GraphicsState state = g.Save();
                        float centerX = colX + currentW / 2f;
                        float centerY = y + 52 + stampBoxH / 2f;
                        g.TranslateTransform(centerX, centerY);
                        g.RotateTransform(-3.5f); // 擬真微傾落章角度
                        g.DrawImage(stampImg, new Rectangle(-105, -105, 210, 210));
                        g.Restore(state);
                    }
                }
                else
                {
                    g.DrawString("（待審核落章）", subTitleFont, new SolidBrush(Color.FromArgb(203, 213, 225)), stampRect, sfCenter);
                }
            }

            // 轉為高品質 JPEG Bytes
            using (MemoryStream msJpg = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encParams = new EncoderParameters(1);
                encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                pageBmp.Save(msJpg, jpgEncoder, encParams);
                jpegBytes = msJpg.ToArray();
            }
        }

        // 將高解析度 A4 影像封裝為原生標準 PDF 1.4 檔案
        WriteJpegAsPdf14(outputPdfPath, jpegBytes, pageW, pageH);
    }

    private static void WriteJpegAsPdf14(string outputPdfPath, byte[] jpegBytes, int pixelW, int pixelH)
    {
        using (FileStream fs = new FileStream(outputPdfPath, FileMode.Create, FileAccess.Write))
        {
            long[] offsets = new long[6];
            WriteAscii(fs, "%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

            offsets[1] = fs.Position;
            WriteAscii(fs, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            offsets[2] = fs.Position;
            WriteAscii(fs, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

            offsets[3] = fs.Position;
            WriteAscii(fs, "3 0 obj\n<< /Type /Page /Parent 2 0 R /Resources << /XObject << /Im0 4 0 R >> >> /MediaBox [0 0 595 842] /Contents 5 0 R >>\nendobj\n");

            offsets[4] = fs.Position;
            WriteAscii(fs, string.Format(
                "4 0 obj\n<< /Type /XObject /Subtype /Image /Width {0} /Height {1} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {2} >>\nstream\n",
                pixelW, pixelH, jpegBytes.Length));
            fs.Write(jpegBytes, 0, jpegBytes.Length);
            WriteAscii(fs, "\nendstream\nendobj\n");

            string contentStream = "q 595 0 0 842 0 0 cm /Im0 Do Q";
            offsets[5] = fs.Position;
            WriteAscii(fs, string.Format("5 0 obj\n<< /Length {0} >>\nstream\n{1}\nendstream\nendobj\n",
                contentStream.Length, contentStream));

            long xrefPos = fs.Position;
            WriteAscii(fs, "xref\n0 6\n0000000000 65535 f \n");
            for (int i = 1; i <= 5; i++)
            {
                WriteAscii(fs, string.Format("{0:D10} 00000 n \n", offsets[i]));
            }

            WriteAscii(fs, string.Format("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{0}\n%%EOF", xrefPos));
        }
    }

    private static void WriteAscii(Stream s, string text)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        s.Write(bytes, 0, bytes.Length);
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
        {
            if (codec.FormatID == format.Guid) return codec;
        }
        return null;
    }
}
