﻿using BaseClasses;
using MATH;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EXPIMP
{
    public class CMainReportExport
    {

        //private const string fontFamily = "Verdana";
        //private const string fontFamily = "Times New Roman";
        private const string fontFamily = "Calibri";
        private const int fontSizeTitle = 14;
        private const int fontSizeNormal = 12;
        private const int fontSizeLegend = 10;

        private static XPdfFontOptions options;
        //private static PdfDocument document = null;

        public static void ReportAllDataToPDFFile(Viewport3D viewPort, CModelData modelData)
        {
            // Set font encoding to unicode
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

            PdfDocument s_document = new PdfDocument();
            s_document.Info.Title = "Export from software";
            //s_document.Info.Author = "";
            //s_document.Info.Subject = "Created with code snippets that show the use of graphical functions";
            //s_document.Info.Keywords = "PDFsharp, XGraphics";
            PdfPage page = s_document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Vykreslenie zobrazovanych textov a objektov do PDF - zoradene z hora
            //DrawLogo(gfx);
            //DrawProjectInfo(gfx,GetProjectInfo());

            DrawModel3D(gfx, viewPort);
            //gfx.Dispose();
                                    
            DrawModelViews(s_document, modelData);
            
            //page = s_document.AddPage();
            //gfx = XGraphics.FromPdfPage(page);
            //DrawBasicGeometry(gfx, modelData);

            //page = s_document.AddPage();
            //gfx = XGraphics.FromPdfPage(page);
            //AddBasicGeometryToDocument(gfx, modelData, 10);


            //DrawCanvas_PDF(canvas, page, canvas.RenderSize.Width);

            //double height = DrawCanvasImage(gfx, canvas);
            //DrawImage(gfx);

            // Create demonstration pages
            //new LinesAndCurves().DrawPage(s_document.AddPage());
            //new Shapes().DrawPage(s_document.AddPage());
            //new Paths().DrawPage(s_document.AddPage());
            //new Text().DrawPage(s_document.AddPage());
            //new Images().DrawPage(s_document.AddPage());

            //PdfPage page2 = s_document.AddPage();
            //XGraphics gfx2 = XGraphics.FromPdfPage(page2);
            //AddTableToDocument(gfx2, 50, tableParams);

            string fileName = GetReportPDFName();
            // Save the s_document...
            s_document.Save(fileName);
            // ...and start a viewer
            Process.Start(fileName);
        }

        /// <summary>
        /// Draw scaled 3Model to PDF
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="viewPort"></param>
        private static void DrawModel3D(XGraphics gfx, Viewport3D viewPort)
        {
            XFont fontBold = new XFont(fontFamily, fontSizeTitle, XFontStyle.Bold, options);
            gfx.DrawString("Structural model in 3D environment: ", fontBold, XBrushes.Black, 20, 280);

            XImage image = XImage.FromBitmapSource(ExportHelper.SaveViewPortContentAsImage(viewPort));            
            double scaleFactor = gfx.PageSize.Width / image.PointWidth;
            double scaledImageWidth = gfx.PageSize.Width;
            double scaledImageHeight = image.PointHeight * scaleFactor;

            gfx.DrawImage(image, 0, 300, scaledImageWidth, scaledImageHeight);
        }

        private static void DrawModelViews(PdfDocument s_document, CModelData data)
        {
            XGraphics gfx;
            PdfPage page;            
            double scale = 1;
            DisplayOptions opts = data.DisplayOptions;
            opts.bUseOrtographicCamera = true;
            opts.bColorsAccordingToMembers = false;
            opts.bColorsAccordingToSections = true;

            List<EViewModelMemberFilters> list_views = new List<EViewModelMemberFilters>()
             { EViewModelMemberFilters.FRONT, EViewModelMemberFilters.BACK, EViewModelMemberFilters.LEFT, EViewModelMemberFilters.RIGHT, EViewModelMemberFilters.TOP, /*EViewModelMemberFilters.BOTTOM,*/ EViewModelMemberFilters.MIDDLE_FRAME,};

            int legendImgWidth = 100;
            int legendTextWidth = 60;

            foreach (EViewModelMemberFilters viewMembers in list_views)
            {
                page = s_document.AddPage();
                page.Size = PageSize.A3;
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                gfx = XGraphics.FromPdfPage(page);
                //DrawImage(gfx, ConfigurationManager.AppSettings["logoAndDetails"], 0, (int)page.Height.Point - 80, 320, 75);
                DrawPDFLogo(gfx, 0, (int)page.Height.Point - 90);

                opts.ModelView = GetView(viewMembers);
                opts.ViewModelMembers = (int)viewMembers;

                CModel filteredModel = null;
                Viewport3D viewPort = ExportHelper.GetBaseModelViewPort(opts, data.Model, out filteredModel);
                viewPort.UpdateLayout();

                DrawCrscLegend(gfx, filteredModel, (int)page.Width.Point - legendImgWidth, legendTextWidth);

                XFont fontBold = new XFont(fontFamily, fontSizeTitle, XFontStyle.Bold, options);
                gfx.DrawString( $"{(viewMembers).ToString()}:", fontBold, XBrushes.Black, 20, 20);
                                
                XImage image = XImage.FromBitmapSource(ExportHelper.RenderVisual(viewPort, scale));


                double scaleFactor = (gfx.PageSize.Width - legendImgWidth - legendTextWidth) / image.PointWidth;
                double scaledImageWidth = gfx.PageSize.Width - legendImgWidth - legendTextWidth;
                double scaledImageHeight = image.PointHeight * scaleFactor;

                gfx.DrawImage(image, 0, 0, scaledImageWidth, scaledImageHeight);

                gfx.Dispose();
            }
        }

        private static int GetView(EViewModelMemberFilters viewModelMembers)
        {
            if (viewModelMembers == EViewModelMemberFilters.FRONT) return (int)EModelViews.FRONT;
            else if (viewModelMembers == EViewModelMemberFilters.BACK) return (int)EModelViews.BACK;
            else if (viewModelMembers == EViewModelMemberFilters.MIDDLE_FRAME) return (int)EModelViews.FRONT;
            else if (viewModelMembers == EViewModelMemberFilters.LEFT) return (int)EModelViews.LEFT;
            else if (viewModelMembers == EViewModelMemberFilters.RIGHT) return (int)EModelViews.RIGHT;
            else if (viewModelMembers == EViewModelMemberFilters.TOP) return (int)EModelViews.TOP;
            //else if (viewModelMembers == EViewModelMemberFilters.BOTTOM) return (int)EModelViews.BOTTOM;
            else return (int)EModelViews.FRONT;
        }

        private static void DrawLogo(XGraphics gfx)
        {
            XImage image = XImage.FromFile(ConfigurationManager.AppSettings["logoForPDF"]);
            gfx.DrawImage(image, 10, 10, 300, 200);
        }

        private static void DrawPDFLogo(XGraphics gfx, int x, int y)
        {
            int width = 240;
            int height = 75;
            XFont font = new XFont(fontFamily, fontSizeLegend, XFontStyle.Regular, options);

            DrawImage(gfx, ConfigurationManager.AppSettings["logo2"], x, y, width, height);

            int textX = x + width;
            int textY = y + 6;
            int lineHeight = 11;

            gfx.DrawString("Formsteel Technologies", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("2 - 4 Waokauri Place", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("Mangere, Auckland", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("tel 09 275 0089", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("fax 09 257 2650", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("free 0800 800 003", font, XBrushes.Black, textX, textY);
            textY += lineHeight;
            gfx.DrawString("sales@formsteel.co.nz", font, XBrushes.Blue, textX, textY);
            textY += lineHeight;
            gfx.DrawString("www.formsteel.co.nz", font, XBrushes.Blue, textX, textY);
            //Adding web link in older versions
            //PdfSharp.Drawing.XRect rect2 = gfx.Transformer.WorldToDefaultPage(new PdfSharp.Drawing.XRect(new PdfSharp.Drawing.XPoint(textX, textY), new XSize(100, 12)));
            //PdfSharp.Pdf.PdfRectangle rc2 = new PdfSharp.Pdf.PdfRectangle(rect2);
            //gfx.PdfPage.AddWebLink(rc2, "www.formsteel.co.nz");
        }

        private static void DrawImage(XGraphics gfx, string path, int x, int y, int width, int height)
        {
            XImage image = XImage.FromFile(path);
            gfx.DrawImage(image, x, y, width, height);
        }

        private static void DrawCrscLegend(XGraphics gfx, CModel model, int x, int textWidth)
        {
            int width = 100;
            int height = 76;            
            int y = 20;
            int font_y = 20;
            
            XFont font = new XFont(fontFamily, fontSizeLegend, XFontStyle.Regular, options);
            List<string> list_crsc = GetCrscFromModel(model);
            
            foreach (string crsc in list_crsc)
            {
                DrawImage(gfx, ConfigurationManager.AppSettings[crsc], x, y, width, height);

                List<string> list_memberTypes = GetMemberTypesWithCrscFromModel(model, crsc);
                font_y = 20;
                foreach (string s in list_memberTypes)
                {                    
                    gfx.DrawString($"[{s}]", font, XBrushes.Black, x - textWidth, y + font_y);
                    font_y += 15;
                }
                gfx.DrawString($"{crsc}", font, XBrushes.Black, x - textWidth, y + font_y);
                y += height;
            }
        }

        private static List<string> GetCrscFromModel(CModel model)
        {
            List<string> list_crsc = new List<string>();
            foreach (CMember m in model.m_arrMembers)
            {
                if (m.CrScStart != null)
                {
                    if (!list_crsc.Contains(m.CrScStart.Name_short)) list_crsc.Add(m.CrScStart.Name_short);
                }
                if (m.CrScEnd != null)
                {
                    if (!list_crsc.Contains(m.CrScEnd.Name_short)) list_crsc.Add(m.CrScEnd.Name_short);
                }
            }
            return list_crsc;
        }

        private static List<string> GetMemberTypesWithCrscFromModel(CModel model, string crscName)
        {
            List<string> list_memberTypes = new List<string>();
            foreach (CMember m in model.m_arrMembers)
            {
                if (m.CrScStart != null && m.CrScStart.Name_short == crscName)
                {
                    if (!list_memberTypes.Contains(m.EMemberTypePosition.GetFriendlyName())) list_memberTypes.Add(m.EMemberTypePosition.GetFriendlyName());
                }
                if (m.CrScEnd != null && m.CrScEnd.Name_short == crscName)
                {
                    if (!list_memberTypes.Contains(m.EMemberTypePosition.GetFriendlyName())) list_memberTypes.Add(m.EMemberTypePosition.GetFriendlyName());
                }
            }
            return list_memberTypes;
        }

        private static CProjectInfo GetProjectInfo()
        {
            CProjectInfo pInfo = new CProjectInfo("New self storage", "8 Forest Road, Stoke", "B6351", "Building 1", DateTime.Now);
            return pInfo;
        }

        private static void DrawProjectInfo(XGraphics gfx, CProjectInfo pInfo)
        {           
            XFont font = new XFont(fontFamily, fontSizeTitle, XFontStyle.Regular, options);
            XFont fontBold = new XFont(fontFamily, fontSizeTitle, XFontStyle.Bold, options);

            int offsetX1 = 320;
            int offsetX2 = 510;
            gfx.DrawString("Project Name: ", font, XBrushes.Black, offsetX1, 20);
            if (pInfo.ProjectName != null) gfx.DrawString(pInfo.ProjectName, fontBold, XBrushes.Black, offsetX1, 40);

            gfx.DrawString("Site: ", font, XBrushes.Black, offsetX1, 60);
            if (pInfo.Site != null) gfx.DrawString(pInfo.Site, fontBold, XBrushes.Black, offsetX1, 80);

            gfx.DrawString("Project Number: ", font, XBrushes.Black, offsetX1, 140);
            if (pInfo.ProjectNumber != null) gfx.DrawString(pInfo.ProjectNumber, fontBold, XBrushes.Black, offsetX1, 160);

            gfx.DrawString("Project Part: ", font, XBrushes.Black, offsetX2, 20);
            gfx.DrawString(pInfo.ProjectPart, fontBold, XBrushes.Black, offsetX2, 40);

            gfx.DrawString("Date: ", font, XBrushes.Black, offsetX2, 140);
            gfx.DrawString(pInfo.Date.ToShortDateString(), fontBold, XBrushes.Black, offsetX2, 160);

        }

        private static void DrawBasicGeometry(XGraphics gfx, CModelData data)
        {
            XFont fontTitle = new XFont(fontFamily, fontSizeTitle, XFontStyle.Bold, options);

            XFont font = new XFont(fontFamily, fontSizeTitle, XFontStyle.Regular, options);
            XFont fontBold = new XFont(fontFamily, fontSizeTitle, XFontStyle.Bold, options);

            int offsetX1 = 20;
            int offsetX2 = 120;
            int offsetX3 = 220;
            int offsetX4 = 320;

            gfx.DrawString("Basic Geometry - Building", fontTitle, XBrushes.Black, offsetX1, 20);


            gfx.DrawString("Width", font, XBrushes.Black, offsetX1, 60);
            gfx.DrawString("B = ", font, XBrushes.Black, offsetX2, 60);
            gfx.DrawString(data.GableWidth.ToString(), font, XBrushes.Black, offsetX3, 60);
            gfx.DrawString("m", font, XBrushes.Black, offsetX4, 60);

            gfx.DrawString("Length", font, XBrushes.Black, offsetX1, 80);
            gfx.DrawString("L", font, XBrushes.Black, offsetX2, 80);
            gfx.DrawString(data.Length.ToString(), font, XBrushes.Black, offsetX3, 80);
            gfx.DrawString("m", font, XBrushes.Black, offsetX4, 80);

            gfx.DrawString("Height", font, XBrushes.Black, offsetX1, 100);
            gfx.DrawString("H1", font, XBrushes.Black, offsetX2, 100);
            gfx.DrawString(data.WallHeight.ToString(), font, XBrushes.Black, offsetX3, 100);
            gfx.DrawString("m", font, XBrushes.Black, offsetX4, 100);

            gfx.DrawString("Height", font, XBrushes.Black, offsetX1, 120);
            gfx.DrawString("???", font, XBrushes.Black, offsetX2, 120);
            gfx.DrawString("H", font, XBrushes.Black, offsetX2, 100);
            gfx.DrawString(data.WallHeight.ToString(), font, XBrushes.Black, offsetX3, 100);
            gfx.DrawString("m", font, XBrushes.Black, offsetX4, 100);

            gfx.DrawString("Pitch", font, XBrushes.Black, offsetX1, 140);
            gfx.DrawString("α", font, XBrushes.Black, offsetX2, 140);
            gfx.DrawString(data.RoofPitch_deg.ToString(), font, XBrushes.Black, offsetX3, 140);
            gfx.DrawString("deg", font, XBrushes.Black, offsetX4, 140);            
        }

        private static void AddBasicGeometryToDocument(XGraphics gfx, CModelData data, double offsetY)
        {
            gfx.MUH = PdfFontEncoding.Unicode;
            //gfx.MFEH = PdfFontEmbedding.Always;

            // You always need a MigraDoc document for rendering.
            Document doc = new Document();
            Section sec = doc.AddSection();
            Paragraph para = sec.AddParagraph();
            para.Format.FirstLineIndent = 0;
            para.Format.LeftIndent = 0;

            para.AddFormattedText("Width\t\t");
            //para.Format.SpaceAfter = "3cm";
            FormattedText t = para.AddFormattedText("B =", TextFormat.Bold);
            t.AddTab();
            para.AddFormattedText(data.GableWidth.ToString());
            para.AddText("m");
            t = para.AddFormattedText("2");
            t.Superscript = true;
            para.AddLineBreak();
            para.AddFormattedText("Pitch\t\t");
            para.AddFormattedText("α\t");
            para.AddFormattedText(data.RoofPitch_deg.ToString());
            para.AddFormattedText("\tdeg");

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfRenderer.Document = doc;
            pdfRenderer.RenderDocument();
            // Create a renderer and prepare (=layout) the document
            MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            docRenderer.RenderObject(gfx, XUnit.FromPoint(40), XUnit.FromPoint(offsetY), XUnit.FromPoint(gfx.PageSize.Width * 0.8), para);
            //docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
        }

        private static string GetReportPDFName()
        {
            int count = 0;
            string fileName = null;
            bool nameOK = false;
            while (!nameOK)
            {
                fileName = $"Report_{++count}.pdf";

                if (!System.IO.File.Exists(fileName)) nameOK = true;
            }
            return fileName;
        }
 




























        private static void DrawFSAddress(XGraphics gfx)
        {
            XFont font = new XFont(fontFamily, 6, XFontStyle.Regular);

            string sLine1 = "Enquires to:";
            string sLine2 = "FS Technologies Ltd";
            string sLine3 = "2-4 Waokauri Pl, Mangere";
            string sLine4 = "P.O.Box 23-718, Auckland";
            string sLine5 = "Telephone 09 275 0089";

            double dposition_x = 100;
            double dposition_y = 755;
            double drowheight = 9.5;

            gfx.DrawString(sLine1, font, XBrushes.Black, dposition_x, dposition_y);
            gfx.DrawString(sLine2, font, XBrushes.Black, dposition_x, dposition_y + 1 * drowheight);
            gfx.DrawString(sLine3, font, XBrushes.Black, dposition_x, dposition_y + 2 * drowheight);
            gfx.DrawString(sLine4, font, XBrushes.Black, dposition_x, dposition_y + 3 * drowheight);
            gfx.DrawString(sLine5, font, XBrushes.Black, dposition_x, dposition_y + 4 * drowheight);
        }

        private static void DrawPlateInfo(XGraphics gfx, CPlate plate)
        {
            string plateNamePrefix = plate.Name;
            string plateName = "";
            decimal plateThickness = (decimal)plate.Ft * 1000; // Convert to mm
            float platePitch_rad = 0;

            if (plate is CConCom_Plate_JA)
            {
                CConCom_Plate_JA plateTemp = (CConCom_Plate_JA)plate;
                plateName = "Apex Plate";
                platePitch_rad = plateTemp.FSlope_rad;
            }
            else if (plate is CConCom_Plate_JB)
            {
                CConCom_Plate_JB plateTemp = (CConCom_Plate_JB)plate;
                plateName = "Apex Plate";
                platePitch_rad = plateTemp.FSlope_rad;
            }
            else if (plate is CConCom_Plate_JBS)
            {
                CConCom_Plate_JBS plateTemp = (CConCom_Plate_JBS)plate;
                plateName = "Apex Plate";
                platePitch_rad = plateTemp.FSlope_rad;
            }
            else if (plate is CConCom_Plate_KA)
            {
                CConCom_Plate_KA plateTemp = (CConCom_Plate_KA)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KB)
            {
                CConCom_Plate_KB plateTemp = (CConCom_Plate_KB)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KBS)
            {
                CConCom_Plate_KBS plateTemp = (CConCom_Plate_KBS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KC)
            {
                CConCom_Plate_KC plateTemp = (CConCom_Plate_KC)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KCS)
            {
                CConCom_Plate_KCS plateTemp = (CConCom_Plate_KCS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KD)
            {
                CConCom_Plate_KD plateTemp = (CConCom_Plate_KD)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KDS)
            {
                CConCom_Plate_KDS plateTemp = (CConCom_Plate_KDS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KES)
            {
                CConCom_Plate_KES plateTemp = (CConCom_Plate_KES)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KFS)
            {
                CConCom_Plate_KFS plateTemp = (CConCom_Plate_KFS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KK)
            {
                CConCom_Plate_KK plateTemp = (CConCom_Plate_KK)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else
            {
                // Not defined
                platePitch_rad = 0;
                plateName = "";
            }

            decimal platePitch = (decimal)Math.Round(Geom2D.RadiansToDegrees(Math.Abs(platePitch_rad)), 1); // Display absolute value in deg, 1 decimal place

            XFont font1 = new XFont(fontFamily, 14, XFontStyle.Bold);
            XFont font2 = new XFont(fontFamily, 12, XFontStyle.Regular);

            XTextFormatter tf = new XTextFormatter(gfx);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(plateNamePrefix + " (" + plateName + ")", font1, XBrushes.Black, new XRect(0, 20, gfx.PageSize.Width, 40));

            gfx.DrawString(Math.Round(plateThickness, 2).ToString(), font2, XBrushes.Black, 50, 730);
            gfx.DrawString("mm Plate", font2, XBrushes.Black, 80, 730);
            gfx.DrawString(platePitch.ToString(), font2, XBrushes.Black, 485, 730);
            gfx.DrawString("° Pitch", font2, XBrushes.Black, 513, 730);
        }

        private static double DrawCanvasImage(XGraphics gfx, Canvas canvas)
        {
            try
            {
                XImage image = XImage.FromBitmapSource(GetBitmapSourceFromCanvas(canvas));
                double scaleFactor = gfx.PageSize.Width / image.PointWidth;
                double scaledImageWidth = gfx.PageSize.Width;
                double scaledImageHeight = image.PointHeight * scaleFactor;

                gfx.DrawImage(image, 0, 0, scaledImageWidth, scaledImageHeight);
                return scaledImageHeight;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private static BitmapSource GetBitmapSourceFromCanvas(Canvas canvas)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            return rtb;
        }

        private static void SaveImageFromCanvas(Canvas canvas)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);

            var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = System.IO.File.OpenWrite("ImageFromCanvas.png"))
            {
                pngEncoder.Save(fs);
            }
        }


        private static void DrawImage(XGraphics gfx)
        {
            try
            {
                string jpegSamplePath = "fs-screen.jpg";
                XImage image = XImage.FromFile(jpegSamplePath);
                // Left position in point
                double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
                gfx.DrawImage(image, x, 0);
            }
            catch (Exception ex)
            {

            }
        }

        private static void AddTableToDocument(XGraphics gfx, double offsetY, List<string[]> tableParams)
        {
            gfx.MUH = PdfFontEncoding.Unicode;
            //gfx.MFEH = PdfFontEmbedding.Always;

            // You always need a MigraDoc document for rendering.
            Document doc = new Document();
            Table t = GetSimpleTable(doc, tableParams);

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfRenderer.Document = doc;
            pdfRenderer.RenderDocument();
            // Create a renderer and prepare (=layout) the document
            MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            docRenderer.RenderObject(gfx, XUnit.FromPoint(40), XUnit.FromPoint(offsetY), XUnit.FromPoint(gfx.PageSize.Width * 0.8), t);
            //docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
        }

        private static void AddPlatesTableToDocument(XGraphics gfx, double offsetY, List<string[]> tableParams)
        {
            gfx.MUH = PdfFontEncoding.Unicode;
            //gfx.MFEH = PdfFontEmbedding.Always;

            // You always need a MigraDoc document for rendering.
            Document doc = new Document();
            Table t = GetPlatesParamsTable(doc, gfx, tableParams);

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfRenderer.Document = doc;
            pdfRenderer.RenderDocument();
            // Create a renderer and prepare (=layout) the document
            MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            docRenderer.RenderObject(gfx, XUnit.FromPoint(40), XUnit.FromPoint(offsetY), XUnit.FromPoint(gfx.PageSize.Width * 0.8), t);
            //docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
        }

        public static Table GetSimpleTable(Document document, List<string[]> tableParams)
        {
            Section sec = document.AddSection();
            Table table = new Table();
            table.Borders.Width = 0.75;
            table.Format.Font.Name = fontFamily;

            Column column1 = table.AddColumn(Unit.FromCentimeter(7));
            column1.Format.Alignment = ParagraphAlignment.Left;
            Column column2 = table.AddColumn(Unit.FromCentimeter(2));
            column2.Format.Alignment = ParagraphAlignment.Left;
            Column column3 = table.AddColumn(Unit.FromCentimeter(4));
            column3.Format.Alignment = ParagraphAlignment.Right;
            Column column4 = table.AddColumn(Unit.FromCentimeter(2));
            column4.Format.Alignment = ParagraphAlignment.Left;

            foreach (string[] strParams in tableParams)
            {
                Row row = table.AddRow();
                //row.Shading.Color = Colors.PaleGoldenrod;
                Cell cell = row.Cells[0];
                cell.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                cell.AddParagraph(strParams[0]);
                cell = row.Cells[1];
                cell.AddParagraph(strParams[1]);
                cell = row.Cells[2];
                cell.AddParagraph(strParams[2]);
                cell = row.Cells[3];
                cell.AddParagraph(strParams[3]);
            }

            table.SetEdge(0, 0, 4, tableParams.Count, Edge.Box, BorderStyle.Single, 1.5, MigraDoc.DocumentObjectModel.Colors.Black);
            sec.Add(table);
            return table;
        }

        public static Table GetPlatesParamsTable(Document document, XGraphics gfx, List<string[]> tableParams)
        {
            Section sec = document.AddSection();
            Table table = new Table();
            table.Borders.Width = 0.75;
            table.Format.Font.Name = fontFamily;

            //{ "ID", "Name", "Width", "Height", "Thickness", "Area", "Volume", "Mass", "Amount", "Amount Left", "Amount Right", "Mass Total", "Screws Plate", "Screws Total" };
            Column columnID = table.AddColumn(Unit.FromCentimeter(0.7));
            columnID.Format.Alignment = ParagraphAlignment.Center;
            Column columnName = table.AddColumn(Unit.FromCentimeter(1.1));
            columnName.Format.Alignment = ParagraphAlignment.Center;
            Column columnWidth = table.AddColumn(Unit.FromCentimeter(1.2));
            columnWidth.Format.Alignment = ParagraphAlignment.Center;
            Column columnHeight = table.AddColumn(Unit.FromCentimeter(1.2));
            columnHeight.Format.Alignment = ParagraphAlignment.Center;
            Column columnThickness = table.AddColumn(Unit.FromCentimeter(1.6));
            columnThickness.Format.Alignment = ParagraphAlignment.Center;
            Column columnArea = table.AddColumn(Unit.FromCentimeter(1.1));
            columnArea.Format.Alignment = ParagraphAlignment.Center;
            Column columnVolume = table.AddColumn(Unit.FromCentimeter(1.3));
            columnVolume.Format.Alignment = ParagraphAlignment.Center;
            Column columnMass = table.AddColumn(Unit.FromCentimeter(1.1));
            columnMass.Format.Alignment = ParagraphAlignment.Center;
            Column columnAmount = table.AddColumn(Unit.FromCentimeter(1.5));
            columnAmount.Format.Alignment = ParagraphAlignment.Center;
            Column columnAmountL = table.AddColumn(Unit.FromCentimeter(1.5));
            columnAmountL.Format.Alignment = ParagraphAlignment.Center;
            Column columnAmountR = table.AddColumn(Unit.FromCentimeter(1.5));
            columnAmountR.Format.Alignment = ParagraphAlignment.Center;
            Column columnMassTotal = table.AddColumn(Unit.FromCentimeter(1.8));
            columnMassTotal.Format.Alignment = ParagraphAlignment.Center;

            Column columnAmountScrewPlate = table.AddColumn(Unit.FromCentimeter(1.25));
            columnAmountScrewPlate.Format.Alignment = ParagraphAlignment.Center;
            Column columnAmountScrewTotal = table.AddColumn(Unit.FromCentimeter(1.25));
            columnAmountScrewTotal.Format.Alignment = ParagraphAlignment.Center;

            int columns = 0;
            int count = 0;
            foreach (string[] strParams in tableParams)
            {
                count++;
                Row row = table.AddRow();
                if (count == 1 || count == tableParams.Count) row.Format.Font.Bold = true;

                if (count > 1)
                {
                    row.Format.Alignment = ParagraphAlignment.Right;
                    row.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                }

                //row.Shading.Color = Colors.PaleGoldenrod;
                Cell cell = row.Cells[0];
                cell.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                cell.AddParagraph(strParams[0]);
                cell = row.Cells[1];
                cell.Shading.Color = MigraDoc.DocumentObjectModel.Colors.PaleGoldenrod;
                cell.AddParagraph(strParams[1]);

                // Insert column data ID 2 - 10
                for (int i = 2; i < strParams.Length - 3; i++)
                {
                    cell = row.Cells[i];

                    cell.AddParagraph(strParams[i]);
                }

                cell = row.Cells[11];
                cell.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightCyan;
                cell.AddParagraph(strParams[11]);

                cell = row.Cells[12];
                cell.AddParagraph(strParams[12]);

                cell = row.Cells[13];
                cell.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightCyan;
                cell.AddParagraph(strParams[13]);

                columns = strParams.Length;
            }

            table.SetEdge(0, 0, columns, tableParams.Count, Edge.Box, BorderStyle.Single, 1.5, MigraDoc.DocumentObjectModel.Colors.Black);
            sec.Add(table);
            return table;
        }
    }
}
