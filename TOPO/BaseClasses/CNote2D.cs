﻿using System;
using System.Windows;
using System.Windows.Media;

namespace BaseClasses
{
    public class CNote2D
    {
        private Point MNoteTextPoint;
        private string MText;        
        private bool MDrawArrow;
        private bool MDrawLineUnderText;
        private double MLineThickness;
        private SolidColorBrush MArrowFillColor;
        private SolidColorBrush MArrowStrokeColor;
        private Point MArrowPoint1;
        private Point MArrowPoint2;
        private Point MLineEndPoint;
        private Point MRefPoint;
        private VerticalAlignment MValign;
        private HorizontalAlignment MHalign;
        private double MFontSize;
        private bool MUseRelativePositions;

        public Point NoteTextPoint
        {
            get
            {
                return MNoteTextPoint;
            }

            set
            {
                MNoteTextPoint = value;
            }
        }
        
        public bool DrawArrow
        {
            get
            {
                return MDrawArrow;
            }

            set
            {
                MDrawArrow = value;
            }
        }

        public bool DrawLineUnderText
        {
            get
            {
                return MDrawLineUnderText;
            }

            set
            {
                MDrawLineUnderText = value;
            }
        }

        public Point ArrowPoint1
        {
            get
            {
                return MArrowPoint1;
            }

            set
            {
                MArrowPoint1 = value;
            }
        }

        public Point ArrowPoint2
        {
            get
            {
                return MArrowPoint2;
            }

            set
            {
                MArrowPoint2 = value;
            }
        }

        public Point LineEndPoint
        {
            get
            {
                return MLineEndPoint;
            }

            set
            {
                MLineEndPoint = value;
            }
        }

        public string Text
        {
            get
            {
                return MText;
            }

            set
            {
                MText = value;
            }
        }

        public Point RefPoint
        {
            get
            {
                return MRefPoint;
            }

            set
            {
                MRefPoint = value;
            }
        }

        public VerticalAlignment Valign
        {
            get
            {
                return MValign;
            }

            set
            {
                MValign = value;
            }
        }

        public HorizontalAlignment Halign
        {
            get
            {
                return MHalign;
            }

            set
            {
                MHalign = value;
            }
        }

        public bool UseRelativePositions
        {
            get
            {
                return MUseRelativePositions;
            }

            set
            {
                MUseRelativePositions = value;
            }
        }

        public double FontSize
        {
            get
            {
                return MFontSize;
            }

            set
            {
                MFontSize = value;
            }
        }

        public double LineThickness
        {
            get
            {
                return MLineThickness;
            }

            set
            {
                MLineThickness = value;
            }
        }

        public SolidColorBrush ArrowFillColor
        {
            get
            {
                return MArrowFillColor;
            }

            set
            {
                MArrowFillColor = value;
            }
        }

        public SolidColorBrush ArrowStrokeColor
        {
            get
            {
                return MArrowStrokeColor;
            }

            set
            {
                MArrowStrokeColor = value;
            }
        }

        //----------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------
        public CNote2D() { }
        public CNote2D(Point noteTextPoint, string text, bool drawArrow, Point arrowPoint1, Point arrowPoint2, Point refPoint, SolidColorBrush arrowFillColor, SolidColorBrush arrowStrokeColor,
            bool bDrawLineUnderText = false, VerticalAlignment valign = VerticalAlignment.Center, HorizontalAlignment halign = HorizontalAlignment.Center, 
            double fontSize = 12, bool useRelativePositions = false, double lineThickness = 1)
        {
            MNoteTextPoint = noteTextPoint;
            MText = text;
            //MDistanceX = distanceX; // TO Ondrej - co nastavuju tieto dve hodnoty ? Offset stredu textu od bodu?
            //MDistanceY = distanceY; // TO Ondrej - co nastavuju tieto dve hodnoty ? Offset stredu textu od bodu?
            MDrawArrow = drawArrow;
            MArrowPoint1 = arrowPoint1;
            MArrowPoint2 = arrowPoint2;
            MRefPoint = RefPoint;
            MDrawLineUnderText = bDrawLineUnderText;
            MValign = valign;
            Halign = halign;
            FontSize = fontSize;
            UseRelativePositions = useRelativePositions;

            MLineThickness = lineThickness;
            MArrowFillColor = arrowFillColor;
            MArrowStrokeColor = arrowStrokeColor;
        }

        public void MirrorAboutX_ChangeYCoordinates()
        {
            MNoteTextPoint.Y *= -1;
            //MRefPoint.Y *= -1;
            MArrowPoint1.Y *= -1;
            MArrowPoint2.Y *= -1;
            MLineEndPoint.Y *= -1;
        }

        public void UpdatePoints(double minX, double minY, float modelMarginLeft_x, float fmodelMarginTop_y, double dReal_Model_Zoom_Factor)
        {
            if (!UseRelativePositions)
            {
                MNoteTextPoint = new Point(modelMarginLeft_x + dReal_Model_Zoom_Factor * (MNoteTextPoint.X - minX), fmodelMarginTop_y + dReal_Model_Zoom_Factor * (MNoteTextPoint.Y - minY));
                MRefPoint = new Point(modelMarginLeft_x + dReal_Model_Zoom_Factor * (MRefPoint.X - minX), fmodelMarginTop_y + dReal_Model_Zoom_Factor * (MRefPoint.Y - minY));
                MArrowPoint2 = new Point(modelMarginLeft_x + dReal_Model_Zoom_Factor * (MArrowPoint2.X - minX), fmodelMarginTop_y + dReal_Model_Zoom_Factor * (MArrowPoint2.Y - minY));
                MLineEndPoint = new Point(modelMarginLeft_x + dReal_Model_Zoom_Factor * (MLineEndPoint.X - minX), fmodelMarginTop_y + dReal_Model_Zoom_Factor * (MLineEndPoint.Y - minY));
            }

            //ak su relativne pozicie textov, tak bude iba startovaci bod sipky nie relativny
            MArrowPoint1 = new Point(modelMarginLeft_x + dReal_Model_Zoom_Factor * (MArrowPoint1.X - minX), fmodelMarginTop_y + dReal_Model_Zoom_Factor * (MArrowPoint1.Y - minY));

        }

        public void SetRelativePoints(double canvasWidth, double canvasHeight)
        {
            MArrowPoint2 = new Point(NoteTextPoint.X * canvasWidth, NoteTextPoint.Y * canvasHeight);

            MLineEndPoint = new Point(NoteTextPoint.X * canvasWidth + Drawing2D.GetTextWidth(Text, FontSize), NoteTextPoint.Y * canvasHeight);

            MNoteTextPoint = new Point(NoteTextPoint.X * canvasWidth, NoteTextPoint.Y * canvasHeight - Drawing2D.GetTextHeight(Text, FontSize) / 2);
        }
    }
}
