﻿using BaseClasses;
using MATH;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EXPIMP
{
    public static class CExportToPDF
    {
        //private const string fontFamily = "Verdana";
        //private const string fontFamily = "Times New Roman";
        private const string fontFamily = "Calibri";

        private static PdfDocument document = null;

        private static string GetPDFNameForPlate(CPlate plate)
        {
            float fUnitFactor = 1000; // defined in m, exported in mm
            int count = 0;
            string fileName = null;
            bool nameOK = false;
            while (!nameOK)
            {
                count++;
                fileName = string.Format("{0}_{1}x{2}_{3:D3}.pdf", GetPlateSerieName(plate), Math.Round(plate.Width_bx * fUnitFactor, 3), Math.Round(plate.Height_hy * fUnitFactor, 3), count);

                if (!System.IO.File.Exists(fileName)) nameOK = true;
            }
            return fileName;
        }
        
        private static string GetPlateSerieName(CPlate plate)
        {
            if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_J) return "APEX";
            else if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_K) return "KNEE";
            else if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_O) return "KNEE FACE";
            else return "PLATE";
        }

        public static void CreatePDFFileForPlate(Canvas canvas, List<string[]> tableParams, CPlate plate, CProductionInfo pInfo)
        {
            PdfDocument s_document = new PdfDocument();
            s_document.Info.Title = "Export from software";
            //s_document.Info.Author = "";
            //s_document.Info.Subject = "Created with code snippets that show the use of graphical functions";
            //s_document.Info.Keywords = "PDFsharp, XGraphics";
            PdfPage page = s_document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Vykreslenie zobrazovanych textov a objektov do PDF - zoradene z hora
            DrawProductionInfo(gfx, pInfo, plate);
            DrawPlateInfo(gfx, plate);
            Draw3DScheme(gfx, pInfo, plate);
            if(plate.ScrewArrangement != null && plate.ScrewArrangement.IHolesNumber > 0)
                DrawProductionNotes(gfx);
            //DrawLogo_Old(gfx);
            DrawLogo_New(gfx);
            DrawFSAddress(gfx);
            gfx.Dispose();

            DrawCanvas_PDF(canvas, page, canvas.RenderSize.Width);

            //double height = DrawCanvasImage(gfx, canvas);
            //DrawImage(gfx);

            // Create demonstration pages
            //new LinesAndCurves().DrawPage(s_document.AddPage());
            //new Shapes().DrawPage(s_document.AddPage());
            //new Paths().DrawPage(s_document.AddPage());
            //new Text().DrawPage(s_document.AddPage());
            //new Images().DrawPage(s_document.AddPage());

            PdfPage page2 = s_document.AddPage();
            XGraphics gfx2 = XGraphics.FromPdfPage(page2);
            AddTableToDocument(gfx2, 40, 50, tableParams);
            
            string fileName = GetPDFNameForPlate(plate);
            // Save the s_document...
            s_document.Save(fileName);
            // ...and start a viewer
            Process.Start(fileName);
        }

        public static void CreatePDFDocument()
        {
            document = new PdfDocument();
            document.Info.Title = "Export from software";
        }
        public static void AddPlateToPDF(Canvas canvas, double canvasWidth, CPlate plate, CProductionInfo pInfo)
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Vykreslenie zobrazovanych textov a objektov do PDF - zoradene z hora
            DrawProductionInfo(gfx, pInfo, plate);
            DrawPlateInfo(gfx, plate);
            Draw3DScheme(gfx, pInfo, plate);
            if(plate.ScrewArrangement != null && plate.ScrewArrangement.IHolesNumber > 0)
                DrawProductionNotes(gfx);
            //DrawLogo_Old(gfx);
            DrawLogo_New(gfx);
            DrawFSAddress(gfx);
            gfx.Dispose();

            canvas.UpdateLayout();
            DrawCanvas_PDF(canvas, page, canvasWidth);
        }

        public static void AddPlatesParamsTableToDocumentOnNewPage(List<string[]> tableParams)
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            AddPlatesTableToDocument(gfx, 40, 50, tableParams);
        }

        public static void SavePDFDocument(string fileName)
        {
            try
            {
                // Save the s_document...
                document.Save(fileName); // TODO - Ondrej - tu je chyba, ak je subor s rovnakym nazvom ako ma subor ktory je otvoreny v ADOBE, nemoze sa don zapisovat
                // ...and start a viewer
                Process.Start(fileName);
                document = null;
            }
            catch (IOException ex)
            {
                // The process cannot access the file 'filename' because it is being used by another process
                MessageBox.Show("The process cannot access the file because it is being used by another process.");
                return;
            }
        }

        public static void DrawCanvas_PDF(Canvas canvas, PdfPage page, double canvasWidth)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double scaleFactor = gfx.PageSize.Width / canvasWidth * 0.9; //90%
            double marginLeft = gfx.PageSize.Width * 0.1 / 2.0;
            double marginTop = gfx.PageSize.Height * 0.3 / 2.97;

            foreach (object o in canvas.Children)
            {
                System.Diagnostics.Trace.WriteLine(o.GetType());

                if (o is Rectangle)
                {
                    Rectangle winRect = o as Rectangle;
                    double x = Canvas.GetLeft(winRect);
                    double y = Canvas.GetTop(winRect);

                    System.Windows.Media.Color c = ((SolidColorBrush)winRect.Fill).Color;
                    XSolidBrush solidBrush = new XSolidBrush(XColor.FromArgb(c.A, c.R, c.G, c.B));
                    gfx.DrawRectangle(solidBrush, x * scaleFactor + marginLeft, y * scaleFactor + marginTop, winRect.Width * scaleFactor, winRect.Height * scaleFactor);
                    //gfx.DrawRectangle(solidBrush, x * scaleFactor + marginLeft, y * scaleFactor + marginTop, winRect.Width, winRect.Height); //width, height scale factor not applied
                }
                else if (o is Polyline)
                {
                    Polyline winPol = o as Polyline;

                    System.Windows.Media.Color c = ((SolidColorBrush)winPol.Stroke).Color;
                    //XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winPol.StrokeThickness * scaleFactor);
                    XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winPol.StrokeThickness); //thickness scalefactor not applied

                    List<XPoint> points = new List<XPoint>();
                    foreach (Point p in winPol.Points)
                    {
                        points.Add(new XPoint(p.X * scaleFactor + marginLeft, p.Y * scaleFactor + marginTop));
                    }
                    gfx.DrawLines(pen, points.ToArray());
                }
                else if (o is System.Windows.Shapes.Path)
                {
                    XGraphicsPath xGrPath = new XGraphicsPath();

                    System.Windows.Shapes.Path winPath = o as System.Windows.Shapes.Path;
                    System.Windows.Media.Color c = ((SolidColorBrush)winPath.Stroke).Color;
                    //XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winPath.StrokeThickness * scaleFactor);
                    XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winPath.StrokeThickness);

                    PathGeometry pathGeom = winPath.Data.GetFlattenedPathGeometry();
                    //PathGeometry pathGeom = winPath.Data.GetOutlinedPathGeometry();
                    
                    foreach (PathFigure pf in pathGeom.Figures)
                    {
                        Point start = pf.StartPoint;
                        foreach (PathSegment ps in pf.Segments)
                        {
                            if (ps is ArcSegment)
                            {
                                ArcSegment arc = (ArcSegment)ps;
                                xGrPath.AddArc(new XPoint(start.X * scaleFactor + marginLeft, start.Y * scaleFactor + marginTop), 
                                    new XPoint(arc.Point.X * scaleFactor + marginLeft, arc.Point.Y * scaleFactor + marginTop), 
                                    new XSize(arc.Size.Width, arc.Size.Height), arc.RotationAngle, arc.IsLargeArc, (XSweepDirection)arc.SweepDirection);
                                
                            }
                            else if (ps is BezierSegment)
                            {
                                BezierSegment bs = (BezierSegment)ps;
                                xGrPath.AddBezier(new XPoint(start.X * scaleFactor + marginLeft, start.Y * scaleFactor + marginTop),
                                    new XPoint(bs.Point1.X * scaleFactor + marginLeft, bs.Point1.Y * scaleFactor + marginTop),
                                    new XPoint(bs.Point2.X * scaleFactor + marginLeft, bs.Point2.Y * scaleFactor + marginTop),
                                    new XPoint(bs.Point3.X * scaleFactor + marginLeft, bs.Point3.Y * scaleFactor + marginTop));
                            }
                            else if (ps is LineSegment)
                            {
                                LineSegment ls = (LineSegment)ps;
                                XPoint p1 = new XPoint(start.X * scaleFactor + marginLeft, start.Y * scaleFactor + marginTop);
                                XPoint p2 = new XPoint(ls.Point.X * scaleFactor + marginLeft, ls.Point.Y * scaleFactor + marginTop);  
                                xGrPath.AddLine(p1, p2);
                            }
                            else if (ps is PolyLineSegment) 
                            {
                                PolyLineSegment pls = (PolyLineSegment)ps;
                                List<XPoint> points = new List<XPoint>();
                                points.Add(new XPoint(start.X * scaleFactor + marginLeft, start.Y * scaleFactor + marginTop));
                                foreach (Point p in pls.Points)
                                {
                                    points.Add(new XPoint(p.X * scaleFactor + marginLeft, p.Y * scaleFactor + marginTop));
                                }
                                xGrPath.AddLines(points.ToArray());
                            }
                        }
                    }
                    gfx.DrawPath(pen, xGrPath);
                }
                else if (o is Ellipse)
                {
                    Ellipse winElipse = o as Ellipse;
                    //double majorAxis = winElipse.Width;
                    //double minorAxis = winElipse.Height;

                    System.Windows.Media.Color c = ((SolidColorBrush)winElipse.Stroke).Color;
                    XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winElipse.StrokeThickness);

                    double x = Canvas.GetLeft(winElipse) - winElipse.StrokeThickness / 2;
                    double y = Canvas.GetTop(winElipse) - winElipse.StrokeThickness / 2;

                    gfx.DrawEllipse(pen, x * scaleFactor + marginLeft, y * scaleFactor + marginTop, winElipse.Width * scaleFactor, winElipse.Height * scaleFactor);
                    //gfx.DrawEllipse(pen, x * scaleFactor + marginLeft, y * scaleFactor + marginTop, winElipse.Width, winElipse.Height);
                }
                else if (o is Line)
                {
                    Line winLine = o as Line;
                    
                    System.Windows.Media.Color c = ((SolidColorBrush)winLine.Stroke).Color;
                    //XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winLine.StrokeThickness * scaleFactor);
                    XPen pen = new XPen(XColor.FromArgb(c.A, c.R, c.G, c.B), winLine.StrokeThickness);

                    if (winLine.StrokeDashArray.Count > 0) { pen.DashStyle = XDashStyle.Dash;
                        double[] dashArray = new double[winLine.StrokeDashArray.Count];
                        for (int i = 0; i < dashArray.Length; i++)
                        {
                            dashArray[i] = winLine.StrokeDashArray[i] * scaleFactor;
                        }
                        pen.DashPattern = dashArray;
                    }
                    

                    gfx.DrawLine(pen, winLine.X1 * scaleFactor + marginLeft, winLine.Y1 * scaleFactor + marginTop, winLine.X2 * scaleFactor + marginLeft, winLine.Y2 * scaleFactor + marginTop);
                }
                else if (o is TextBlock)
                {
                    TextBlock winText = o as TextBlock;
                    double angle = 0;
                    if (winText.RenderTransform is RotateTransform)
                    {
                        RotateTransform rotTrans = (RotateTransform)winText.RenderTransform;
                        //System.Diagnostics.Trace.WriteLine(winText.Text);
                        //System.Diagnostics.Trace.WriteLine(rotTrans.Angle);
                        angle = rotTrans.Angle;
                    }

                    double x = Canvas.GetLeft(winText);
                    //if(Math.Abs(angle) > 45) x += winText.ActualHeight;
                    double y = Canvas.GetTop(winText);
                    //y += winText.FontSize;

                    System.Windows.Media.Color c = ((SolidColorBrush)winText.Foreground).Color;
                    XSolidBrush solidBrush = new XSolidBrush(XColor.FromArgb(c.A, c.R, c.G, c.B));
                    //XFont f = new XFont(winText.FontFamily.ToString(), winText.FontSize * scaleFactor);
                    XFont f = new XFont(winText.FontFamily.ToString(), winText.FontSize / 4 * 3);  //pixels to points
                    //XFont f = new XFont(winText.FontFamily.ToString(), winText.FontSize);  //pixels to points
                    //XPoint p = new XPoint(x * scaleFactor + marginLeft, y * scaleFactor + marginTop);

                    //este by stalo mozno za pokus sa pohrat s rotaciou, ci netreba nahodou robit rotaciu
                    //XPoint p = new XPoint(x * scaleFactor + winText.ActualWidth / 2 * scaleFactor + marginLeft, y * scaleFactor + winText.ActualHeight / 4*3 * scaleFactor + marginTop);
                    XPoint p = new XPoint(x * scaleFactor + marginLeft, y * scaleFactor + winText.ActualHeight / 4 * 3 * scaleFactor + marginTop);
                    //XPoint p = new XPoint(x * scaleFactor + winText.ActualWidth / 2 + marginLeft, y * scaleFactor + winText.ActualHeight / 2  + marginTop);
                    if (Math.Abs(angle) > 45) p.X += winText.ActualHeight * scaleFactor;

                    XGraphicsState state = gfx.Save();
                    gfx.RotateAtTransform(angle, p);
                    gfx.DrawString(winText.Text, f, solidBrush, p);
                    gfx.Restore(state);
                }
            }
            gfx.Dispose();
        }

        public static void DrawLogo_Old(XGraphics gfx)
        {
            XImage image = XImage.FromFile(ConfigurationManager.AppSettings["logoForPDF"]);
            gfx.DrawImage(image, 50, 750);

            XImage image2 = XImage.FromFile(ConfigurationManager.AppSettings["confStampForPDF"]);
            gfx.DrawImage(image2, 220, 750);
        }

        public static void DrawLogo_New(XGraphics gfx)
        {
            XImage image = XImage.FromFile(ConfigurationManager.AppSettings["logo2"]);
            gfx.DrawImage(image, 50, 720);

            XImage image2 = XImage.FromFile(ConfigurationManager.AppSettings["confStampForPDF"]);
            gfx.DrawImage(image2, 420, 750);
        }

        public static string Draw3DScheme(XGraphics gfx, CProductionInfo pInfo, CPlate plate)
        {
            // Display scheme

            XImage image;
            string sFileName = "";
            float platePitch_rad = 0;

            if (plate is CConCom_Plate_JB || plate is CConCom_Plate_JBS)
            {
                sFileName = "JB";
            }
            else if (plate is CConCom_Plate_JCS)
            {
                sFileName = "JC";
            }
            else if (plate is CConCom_Plate_KB)
            {
                CConCom_Plate_KB plateTemp = (CConCom_Plate_KB)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KB_RK";
                else
                    sFileName = "KB_FK";
            }
            else if (plate is CConCom_Plate_KBS)
            {
                CConCom_Plate_KBS plateTemp = (CConCom_Plate_KBS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KB_RK";
                else
                    sFileName = "KB_FK";
            }
            else if (plate is CConCom_Plate_KC)
            {
                CConCom_Plate_KC plateTemp = (CConCom_Plate_KC)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KC_RK";
                else
                    sFileName = "KC_FK";
            }
            else if (plate is CConCom_Plate_KCS)
            {
                CConCom_Plate_KCS plateTemp = (CConCom_Plate_KCS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KC_RK";
                else
                    sFileName = "KC_FK";
            }
            else if (plate is CConCom_Plate_KD)
            {
                CConCom_Plate_KD plateTemp = (CConCom_Plate_KD)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KD_RK";
                else
                    sFileName = "KD_FK";
            }
            else if (plate is CConCom_Plate_KDS)
            {
                CConCom_Plate_KDS plateTemp = (CConCom_Plate_KDS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KD_RK";
                else
                    sFileName = "KD_FK";
            }
            else if (plate is CConCom_Plate_KES)
            {
                CConCom_Plate_KES plateTemp = (CConCom_Plate_KES)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KE_RK";
                else
                    sFileName = "KE_FK";
            }
            else if (plate is CConCom_Plate_KFS)
            {
                CConCom_Plate_KFS plateTemp = (CConCom_Plate_KFS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KF_RK";
                else
                    sFileName = "KF_FK";
            }
            else if (plate is CConCom_Plate_KGS)
            {
                CConCom_Plate_KGS plateTemp = (CConCom_Plate_KGS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KG_RK";
                else
                    sFileName = "KG_FK";
            }
            else if (plate is CConCom_Plate_KHS)
            {
                CConCom_Plate_KHS plateTemp = (CConCom_Plate_KHS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "KH_RK";
                else
                    sFileName = "KH_FK";
            }
            else if (plate is CConCom_Plate_O)
            {
                CConCom_Plate_O plateTemp = (CConCom_Plate_O)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    sFileName = "O_RK";
                else
                    sFileName = "O_FK";
            }
            else
            {
                // Not defined
                sFileName = "";
            }

            if (sFileName != "")
            {
                image = XImage.FromFile(ConfigurationManager.AppSettings[sFileName]);

                if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_J) // J
                    gfx.DrawImage(image, 120, 45);
                else if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_K)
                    gfx.DrawImage(image, 458, 2);
                else // O
                    gfx.DrawImage(image, 458, 2);
            }

            // Display number of plates

            // Set font encoding to unicode
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XFont font = new XFont(fontFamily, 12, XFontStyle.Regular, options);

            if (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_K)
            {
                if (plate is CConCom_Plate_KA) return sFileName;
                if (plate is CConCom_Plate_KK) return sFileName;
                //gfx.DrawString("RH: ", font, XBrushes.Black, 460, 20);
                gfx.DrawString(pInfo.AmountRH.ToString(), font, XBrushes.Black, 486, 75);
                //gfx.DrawString("LH: ", font, XBrushes.Black, 480, 20);
                gfx.DrawString(pInfo.AmountLH.ToString(), font, XBrushes.Black, 546, 58);
            }

            return sFileName;
        }

        public static void DrawFSAddress(XGraphics gfx)
        {
            XFont font = new XFont(fontFamily, 10/*6*/, XFontStyle.Regular);

            string sLine1 = "Enquires to:";
            string sLine2 = "FS Technologies Ltd";
            string sLine3 = "2-4 Waokauri Pl, Mangere";
            string sLine4 = "P.O.Box 23-718, Auckland";
            string sLine5 = "Telephone 09 275 0089";

            double dposition_x = 280; //100; Old
            double dposition_y = 738; //755; Old
            double drowheight = 12; //9.5; Old

            gfx.DrawString(sLine1, font, XBrushes.Black, dposition_x, dposition_y);
            gfx.DrawString(sLine2, font, XBrushes.Black, dposition_x, dposition_y + 1 * drowheight);
            gfx.DrawString(sLine3, font, XBrushes.Black, dposition_x, dposition_y + 2 * drowheight);
            gfx.DrawString(sLine4, font, XBrushes.Black, dposition_x, dposition_y + 3 * drowheight);
            gfx.DrawString(sLine5, font, XBrushes.Black, dposition_x, dposition_y + 4 * drowheight);
        }

        private static void DrawProductionInfo(XGraphics gfx, CProductionInfo pInfo, CPlate plate)
        {
            // Set font encoding to unicode
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XFont font = new XFont(fontFamily, 12, XFontStyle.Regular, options);
            XFont font2 = new XFont(fontFamily, 12, XFontStyle.Underline, options);

            gfx.DrawString("Job Number: ", font, XBrushes.Black, 10, 20);
            if (pInfo.JobNumber != null) gfx.DrawString(pInfo.JobNumber, font, XBrushes.Black, 100, 20);
            gfx.DrawString("Customer: ", font, XBrushes.Black, 10, 40);
            if(pInfo.Customer != null) gfx.DrawString(pInfo.Customer, font, XBrushes.Black, 100, 40);
            gfx.DrawString("Amount: ", font, XBrushes.Black, 10, 60);
            gfx.DrawString(pInfo.Amount.ToString(), font, XBrushes.Black, 100, 60);

            if (!plate.IsSymmetric())
            {
                gfx.DrawString("RH: ", font, XBrushes.Black, 40, 80);
                gfx.DrawString(pInfo.AmountRH.ToString(), font, XBrushes.Black, 100, 80);
                gfx.DrawString("LH: ", font, XBrushes.Black, 40, 100);
                gfx.DrawString(pInfo.AmountLH.ToString(), font, XBrushes.Black, 100, 100);
            }
        }

        public static void DrawProductionNotes(XGraphics gfx)
        {
            XFont font = new XFont(fontFamily, 12, XFontStyle.Regular);

            string sNote1 = "Screw Holes - Pre Drill to Ø = 5.7 mm";
            gfx.DrawString(sNote1, font, XBrushes.Black, 200, 700);
        }

        public static void DrawPlateInfo(XGraphics gfx, CPlate plate)
        {
            string plateNamePrefix = plate.Name;
            string plateName = "";
            decimal plateThickness = (decimal)plate.Ft * 1000; // Convert to mm
            float platePitch_rad = 0;

            if (plate is CConCom_Plate_B_basic)
            {
                CConCom_Plate_B_basic plateTemp = (CConCom_Plate_B_basic)plate;
                plateName = "Base Plate";
            }
            else if (plate is CConCom_Plate_F_or_L && plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_L)
            {
                CConCom_Plate_F_or_L plateTemp = (CConCom_Plate_F_or_L)plate;
                plateName = "Angle Plate";
            }
            else if (plate is CConCom_Plate_LL)
            {
                CConCom_Plate_LL plateTemp = (CConCom_Plate_LL)plate;
                plateName = "Purlin Plate";
            }
            else if (plate is CConCom_Plate_F_or_L && plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_F)
            {
                CConCom_Plate_F_or_L plateTemp = (CConCom_Plate_F_or_L)plate;
                plateName = "Angle Plate - Fly Bracing";
            }
            else if (plate is CConCom_Plate_G)
            {
                CConCom_Plate_G plateTemp = (CConCom_Plate_G)plate;
                plateName = "Wind Post Plate";
            }
            else if (plate is CConCom_Plate_H)
            {
                CConCom_Plate_H plateTemp = (CConCom_Plate_H)plate;
                plateName = "Wind Post Plate";
            }
            else if (plate is CConCom_Plate_Q_T_Y && (plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_Q || plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_T))
            {
                CConCom_Plate_Q_T_Y plateTemp = (CConCom_Plate_Q_T_Y)plate;
                plateName = "Channel Plate - Equal";
            }
            else if (plate is CConCom_Plate_Q_T_Y && plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_Y)
            {
                CConCom_Plate_Q_T_Y plateTemp = (CConCom_Plate_Q_T_Y)plate;
                plateName = "Channel Plate - Unequal";
            }
            else if (plate is CConCom_Plate_JA)
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
            else if (plate is CConCom_Plate_JCS)
            {
                CConCom_Plate_JCS plateTemp = (CConCom_Plate_JCS)plate;
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
            else if (plate is CConCom_Plate_KGS)
            {
                CConCom_Plate_KGS plateTemp = (CConCom_Plate_KGS)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Plate - rising";
                else
                    plateName = "Knee Plate - falling";
            }
            else if (plate is CConCom_Plate_KHS)
            {
                CConCom_Plate_KHS plateTemp = (CConCom_Plate_KHS)plate;
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
            else if (plate is CConCom_Plate_O)
            {
                CConCom_Plate_O plateTemp = (CConCom_Plate_O)plate;
                platePitch_rad = plateTemp.FSlope_rad;

                if (plateTemp.FSlope_rad > 0)
                    plateName = "Knee Face Plate - rising";
                else
                    plateName = "Knee Face Plate - falling";
            }
            else if (plate is CConCom_Plate_M)
            {
                CConCom_Plate_M plateTemp = (CConCom_Plate_M)plate;
                platePitch_rad = plateTemp.RoofPitch_rad;
                plateName = "Wind Post Strip Plate";
            }
            else if (plate is CConCom_Plate_N)
            {
                CConCom_Plate_N plateTemp = (CConCom_Plate_N)plate;
                plateName = "Wind Post Strip Plate";
            }
            else if (plate is CWasher_W)
            {
                CWasher_W plateTemp = (CWasher_W)plate;
                plateName = "Washer";
            }
            else
            {
                // Not defined
                platePitch_rad = 0;
                plateName = "";
            }

            XFont font1 = new XFont(fontFamily, 14, XFontStyle.Bold);
            XFont font2 = new XFont(fontFamily, 12, XFontStyle.Regular);

            XTextFormatter tf = new XTextFormatter(gfx);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(plateNamePrefix + " (" + plateName + ")", font1, XBrushes.Black, new XRect(0, 20, gfx.PageSize.Width, 40));

            gfx.DrawString(Math.Round(plateThickness, 2).ToString(), font2, XBrushes.Black, 50, 730);
            gfx.DrawString("mm Plate", font2, XBrushes.Black, 80, 730);

            // Sklon vypisovat len u plechov serie J, K a O
            if(plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_J || plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_K || plate.m_ePlateSerieType_FS == ESerieTypePlate.eSerie_O)
            {
                if(platePitch_rad != float.NaN) // Valid roof pitch value
                {
                    decimal platePitch = (decimal)Math.Round(Geom2D.RadiansToDegrees(Math.Abs(platePitch_rad)), 1); // Display absolute value in deg, 1 decimal place
                    gfx.DrawString(platePitch.ToString(), font2, XBrushes.Black, 485, 730);
                    gfx.DrawString("° Pitch", font2, XBrushes.Black, 513, 730);
                }
            }
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

        private static void AddTableToDocument(XGraphics gfx, double offsetX, double offsetY, List<string[]> tableParams)
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
            docRenderer.RenderObject(gfx, XUnit.FromPoint(offsetX), XUnit.FromPoint(offsetY), XUnit.FromPoint(gfx.PageSize.Width * 0.8), t);
            //docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
        }

        private static void AddPlatesTableToDocument(XGraphics gfx, double offsetX, double offsetY, List<string[]> tableParams)
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
            docRenderer.RenderObject(gfx, XUnit.FromPoint(offsetX), XUnit.FromPoint(offsetY), XUnit.FromPoint(gfx.PageSize.Width * 0.8), t);
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

                if(count > 1)
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

        //private static void BeginBox(XGraphics gfx, int number, string title)
        //{
        //    const int dEllipse = 15;
        //    XRect rect = new XRect(0, 20, 300, 200);
        //    if (number % 2 == 0)
        //        rect.X = 300 - 5;
        //    rect.Y = 40 + ((number - 1) / 2) * (200 - 5);
        //    rect.Inflate(-10, -10);
        //    XRect rect2 = rect;
        //    rect2.Offset(this.borderWidth, this.borderWidth);
        //    gfx.DrawRoundedRectangle(new XSolidBrush(this.shadowColor), rect2, new XSize(dEllipse + 8, dEllipse + 8));
        //    XLinearGradientBrush brush = new XLinearGradientBrush(rect, this.backColor, this.backColor2, XLinearGradientMode.Vertical);
        //    gfx.DrawRoundedRectangle(this.borderPen, brush, rect, new XSize(dEllipse, dEllipse));
        //    rect.Inflate(-5, -5);

        //    XFont font = new XFont("Verdana", 12, XFontStyle.Regular);
        //    gfx.DrawString(title, font, XBrushes.Navy, rect, XStringFormats.TopCenter);

        //    rect.Inflate(-10, -5);
        //    rect.Y += 20;
        //    rect.Height -= 20;

        //    this.state = gfx.Save();
        //    gfx.TranslateTransform(rect.X, rect.Y);
        //}

        //private void EndBox(XGraphics gfx)
        //{
        //    gfx.Restore(this.state);
        //}
    }
}
