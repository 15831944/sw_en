﻿using System;
using BaseClasses.GraphObj;
using BaseClasses.GraphObj.Objects_3D;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MATH;
using DATABASE;
using DATABASE.DTO;
using MATERIAL;
using System.Collections.Generic;

namespace BaseClasses
{
    [Serializable]
    public class CAnchor : CConnector
    {
        // Anchor to plate edge distances
        private float m_fx_pe_minus;
        private float m_fx_pe_plus;
        private float m_fy_pe_minus;
        private float m_fy_pe_plus;

        private float m_fx_pe_min;
        private float m_fy_pe_min;
        private float m_fx_pe_max;
        private float m_fy_pe_max;

        // Anchor to foundation edge distances
        private float m_fx_fe_minus;
        private float m_fx_fe_plus;
        private float m_fy_fe_minus;
        private float m_fy_fe_plus;

        private float m_fx_fe_min;
        private float m_fy_fe_min;
        private float m_fx_fe_max;
        private float m_fy_fe_max;

        // Washer size
        // Plate washer
        private float m_fx_washer_plate;
        private float m_fy_washer_plate;

        // Bearing washer
        private float m_fx_washer_bearing;
        private float m_fy_washer_bearing;

        private float m_fh_effective; // Effective Depth

        private List<CNut> m_Nuts;

        //-------------------------------------------------------------------------------------------------------------
        public float x_pe_minus
        {
            get
            {
                return m_fx_pe_minus;
            }

            set
            {
                m_fx_pe_minus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_pe_plus
        {
            get
            {
                return m_fx_pe_plus;
            }

            set
            {
                m_fx_pe_plus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_pe_minus
        {
            get
            {
                return m_fy_pe_minus;
            }

            set
            {
                m_fy_pe_minus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_pe_plus
        {
            get
            {
                return m_fy_pe_plus;
            }

            set
            {
                m_fy_pe_plus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_pe_min
        {
            get
            {
                return m_fx_pe_min;
            }

            set
            {
                m_fx_pe_min = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_pe_max
        {
            get
            {
                return m_fx_pe_max;
            }

            set
            {
                m_fx_pe_max = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_pe_min
        {
            get
            {
                return m_fy_pe_min;
            }

            set
            {
                m_fy_pe_min = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_pe_max
        {
            get
            {
                return m_fy_pe_max;
            }

            set
            {
                m_fy_pe_max = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_fe_minus
        {
            get
            {
                return m_fx_fe_minus;
            }

            set
            {
                m_fx_fe_minus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_fe_plus
        {
            get
            {
                return m_fx_fe_plus;
            }

            set
            {
                m_fx_fe_plus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_fe_minus
        {
            get
            {
                return m_fy_fe_minus;
            }

            set
            {
                m_fy_fe_minus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_fe_plus
        {
            get
            {
                return m_fy_fe_plus;
            }

            set
            {
                m_fy_fe_plus = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_fe_min
        {
            get
            {
                return m_fx_fe_min;
            }

            set
            {
                m_fx_fe_min = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_fe_max
        {
            get
            {
                return m_fx_fe_max;
            }

            set
            {
                m_fx_fe_max = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_fe_min
        {
            get
            {
                return m_fy_fe_min;
            }

            set
            {
                m_fy_fe_min = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_fe_max
        {
            get
            {
                return m_fy_fe_max;
            }

            set
            {
                m_fy_fe_max = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_washer_plate
        {
            get
            {
                return m_fx_washer_plate;
            }

            set
            {
                m_fx_washer_plate = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_washer_plate
        {
            get
            {
                return m_fy_washer_plate;
            }

            set
            {
                m_fy_washer_plate = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float x_washer_bearing
        {
            get
            {
                return m_fx_washer_bearing;
            }

            set
            {
                m_fx_washer_bearing = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float y_washer_bearing
        {
            get
            {
                return m_fy_washer_bearing;
            }

            set
            {
                m_fy_washer_bearing = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float h_effective
        {
            get
            {
                return m_fh_effective;
            }

            set
            {
                m_fh_effective = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public List<CNut> Nuts
        {
            get
            {
                return m_Nuts;
            }

            set
            {
                m_Nuts = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        private float m_fDiameter_pitch;
        public float Diameter_pitch
        {
            get
            {
                return m_fDiameter_pitch;
            }

            set
            {
                m_fDiameter_pitch = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        private float m_Area_p_pitch;
        public float Area_p_pitch
        {
            get
            {
                return m_Area_p_pitch;
            }

            set
            {
                m_Area_p_pitch = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        private float m_Price_PPLM_NZD;
        public float Price_PPLM_NZD
        {
            get
            {
                return m_Price_PPLM_NZD;
            }

            set
            {
                m_Price_PPLM_NZD = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        private CWasher_W m_WasherPlateTop;
        public CWasher_W WasherPlateTop
        {
            get
            {
                return m_WasherPlateTop;
            }

            set
            {
                m_WasherPlateTop = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        private CWasher_W m_WasherBearing;
        public CWasher_W WasherBearing
        {
            get
            {
                return m_WasherBearing;
            }

            set
            {
                m_WasherBearing = value;
            }
        }

        public CAnchor() : base()
        {
        }

        public CAnchor(string name_temp, float fLength_temp, /*CWasher_W washerPlateTop, CWasher_W washerBearing,*/ bool bIsDisplayed)
        {
            Prefix = "Anchor";
            Name = name_temp;
            m_pControlPoint = new Point3D(0, 0, 0);
            Length = fLength_temp;

            CBoltProperties properties = CBoltsManager.GetBoltProperties(Name, "ThreadedBars");

            Diameter_shank = (float)properties.ShankDiameter;
            Diameter_thread = (float)properties.ThreadDiameter;
            Diameter_pitch = (float)properties.PitchDiameter;

            Price_PPKG_NZD = (float)properties.Price_PPKG_NZD;
            Price_PPLM_NZD = (float)properties.Price_PPLM_NZD;
            Price_PPP_NZD = (float)properties.Price_PPLM_NZD * fLength_temp;
            Mass = (float)properties.Mass_kg_LM * fLength_temp;

            Area_c_thread = MathF.fPI * MathF.Pow2(Diameter_thread) / 4f; // Core / thread area
            Area_o_shank = MathF.fPI * MathF.Pow2(Diameter_shank) / 4f; // Shank area
            Area_p_pitch = MathF.fPI * MathF.Pow2(Diameter_pitch) / 4f; // Pitch diameter area

            // Washer size
            // Plate washer
            //m_WasherPlateTop = washerPlateTop;
            //x_washer_plate = washerPlateTop.Width_bx; // 80 mm
            //y_washer_plate = washerPlateTop.Height_hy; // 80 mm

            // Bearing washer
            //m_WasherBearing = washerBearing;
            //x_washer_bearing = washerBearing.Width_bx; // 60 mm
            //y_washer_bearing = washerBearing.Height_hy; // 60 mm

            h_effective = 0.90909f * fLength_temp; // 300 mm (efektivna dlzka tyce zabetonovana v zaklade)

            ((CMat_03_00)m_Mat).Name = "8.8";
            ((CMat_03_00)m_Mat).m_ft_interval = new float[1] { 0.100f };

            CMatPropertiesBOLT materialProperties = CMaterialManager.LoadMaterialPropertiesBOLT(m_Mat.Name);

            ((CMat_03_00)m_Mat).m_ff_yk = new float[1] { (float)materialProperties.Fy };
            ((CMat_03_00)m_Mat).m_ff_u = new float[1] { (float)materialProperties.Fu };

            Mass = GetMass();

            BIsDisplayed = bIsDisplayed;

            m_fRotationX_deg = 0;
            m_fRotationY_deg = 90;
            m_fRotationZ_deg = 0;

            m_DiffuseMat = new DiffuseMaterial(Brushes.Azure);
            //m_cylinder = new Cylinder(0.5f * Diameter_shank, Length, m_DiffuseMat);
        }

        public CAnchor(string name_temp, string nameMaterial_temp, float fLength_temp, float fh_eff_temp, /*CWasher_W washerPlateTop, CWasher_W washerBearing,*/ bool bIsDisplayed)
        {
            Prefix = "Anchor";
            Name = name_temp;
            m_pControlPoint = new Point3D(0, 0, 0);
            Length = fLength_temp;

            CBoltProperties properties = CBoltsManager.GetBoltProperties(Name, "ThreadedBars");

            Diameter_shank = (float)properties.ShankDiameter;
            Diameter_thread = (float)properties.ThreadDiameter;
            Diameter_pitch = (float)properties.PitchDiameter;

            Price_PPKG_NZD = (float)properties.Price_PPKG_NZD;
            Price_PPLM_NZD = (float)properties.Price_PPLM_NZD;
            Price_PPP_NZD = (float)properties.Price_PPLM_NZD * fLength_temp;
            Mass = (float)properties.Mass_kg_LM * fLength_temp;

            Area_c_thread = MathF.fPI * MathF.Pow2(Diameter_thread) / 4f; // Core / thread area
            Area_o_shank = MathF.fPI * MathF.Pow2(Diameter_shank) / 4f; // Shank area
            Area_p_pitch = MathF.fPI * MathF.Pow2(Diameter_pitch) / 4f; // Pitch diameter area

            // Washer size
            // Plate washer
            //m_WasherPlateTop = washerPlateTop;
            //x_washer_plate = washerPlateTop.Width_bx; // 80 mm
            //y_washer_plate = washerPlateTop.Height_hy; // 80 mm

            // Bearing washer
            //m_WasherBearing = washerBearing;
            //x_washer_bearing = washerBearing.Width_bx; // 60 mm
            //y_washer_bearing = washerBearing.Height_hy; // 60 mm

            h_effective = fh_eff_temp; // Efektivna dlzka tyce zabetonovana v zaklade

            m_Mat.Name = nameMaterial_temp;
            ((CMat_03_00)m_Mat).m_ft_interval = new float[1] { 0.100f };

            CMatPropertiesBOLT materialProperties = CMaterialManager.LoadMaterialPropertiesBOLT(m_Mat.Name);

            ((CMat_03_00)m_Mat).m_ff_yk = new float[1] { (float)materialProperties.Fy };
            ((CMat_03_00)m_Mat).m_ff_u = new float[1] { (float)materialProperties.Fu };

            Mass = GetMass();

            BIsDisplayed = bIsDisplayed;

            m_fRotationX_deg = 0;
            m_fRotationY_deg = 90;
            m_fRotationZ_deg = 0;

            m_DiffuseMat = new DiffuseMaterial(Brushes.Azure);
            //m_cylinder = new Cylinder(0.5f * Diameter_shank, Length, m_DiffuseMat);
        }

        public CAnchor(string name_temp, string nameMaterial_temp, Point3D controlpoint, float fLength_temp, float fh_eff_temp, CWasher_W washerPlateTop, CWasher_W washerBearing, float fRotation_x_deg, float fRotation_y_deg, float fRotation_z_deg, bool bIsDisplayed)
        {
            Prefix = "Anchor";
            Name = name_temp;
            m_pControlPoint = controlpoint;
            Length = fLength_temp;

            CBoltProperties properties = CBoltsManager.GetBoltProperties(Name, "ThreadedBars");

            Diameter_shank = (float)properties.ShankDiameter;
            Diameter_thread = (float)properties.ThreadDiameter;
            Diameter_pitch = (float)properties.PitchDiameter;

            Price_PPKG_NZD = (float)properties.Price_PPKG_NZD;
            Price_PPLM_NZD = (float)properties.Price_PPLM_NZD;
            Price_PPP_NZD = (float)properties.Price_PPLM_NZD * fLength_temp;
            Mass = (float)properties.Mass_kg_LM * fLength_temp;

            Area_c_thread = MathF.fPI * MathF.Pow2(Diameter_thread) / 4f; // Core / thread area
            Area_o_shank = MathF.fPI * MathF.Pow2(Diameter_shank) / 4f; // Shank area
            Area_p_pitch = MathF.fPI * MathF.Pow2(Diameter_pitch) / 4f; // Pitch diameter area

            // Washer size
            // Plate washer
            if (washerPlateTop != null)
            {
                m_WasherPlateTop = washerPlateTop;
                x_washer_plate = washerPlateTop.Width_bx; // 80 mm
                y_washer_plate = washerPlateTop.Height_hy; // 80 mm

                m_Nuts = new List<CNut>();

                CNut nut = new CNut(name_temp, nameMaterial_temp, new Point3D(0.1, 0, 0), 0, -90, 0, true);
                m_Nuts.Add(nut);
            }

            // Bearing washer
            if (washerBearing != null)
            {
                m_WasherBearing = washerBearing;
                x_washer_bearing = washerBearing.Width_bx; // 60 mm
                y_washer_bearing = washerBearing.Height_hy; // 60 mm

                if(m_Nuts == null)
                    m_Nuts = new List<CNut>();

                CNut nutTop = new CNut(name_temp, nameMaterial_temp, new Point3D(0.20, 0, 0), 0, -90, 0, true);
                CNut nutBottom = new CNut(name_temp, nameMaterial_temp, new Point3D(0.25, 0, 0), 0, -90, 0, true);
                m_Nuts.Add(nutTop);
                m_Nuts.Add(nutBottom);
            }

            h_effective = fh_eff_temp; // Efektivna dlzka tyce zabetonovana v zaklade

            m_Mat.Name = nameMaterial_temp;
            ((CMat_03_00)m_Mat).m_ft_interval = new float[1] { 0.100f };

            CMatPropertiesBOLT materialProperties = CMaterialManager.LoadMaterialPropertiesBOLT(m_Mat.Name);

            ((CMat_03_00)m_Mat).m_ff_yk = new float[1] { (float)materialProperties.Fy };
            ((CMat_03_00)m_Mat).m_ff_u = new float[1] { (float)materialProperties.Fu };

            Mass = GetMass();

            BIsDisplayed = bIsDisplayed;

            m_fRotationX_deg = fRotation_x_deg;
            m_fRotationY_deg = fRotation_y_deg;
            m_fRotationZ_deg = fRotation_z_deg;

            m_DiffuseMat = new DiffuseMaterial(Brushes.Azure);
            //m_cylinder = new Cylinder(0.5f * Diameter_shank, Length, m_DiffuseMat);
        }

        public float GetMass()
        {
            return Area_p_pitch * Length * m_Mat.m_fRho;
        }

        /*
        protected override void loadIndices()
        {

        }

        protected override Point3DCollection GetDefinitionPoints()
        {
            Point3DCollection pointCollection = new Point3DCollection();
            return pointCollection;
        }

        public override GeometryModel3D CreateGeomModel3D(SolidColorBrush brush)
        {
            GeometryModel3D geometryModel = new GeometryModel3D();
            return geometryModel;
        }
        public override ScreenSpaceLines3D CreateWireFrameModel()
        {
            ScreenSpaceLines3D geometryWireFrameModel = new ScreenSpaceLines3D();
            return geometryWireFrameModel;
        }
        */
        }
    }
