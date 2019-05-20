﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;
using System.Globalization;
using BaseClasses;
using MATH.ARRAY;
using DATABASE;
using DATABASE.DTO;

namespace PFD
{
    public class CPFDLoadInput : INotifyPropertyChanged
    {
        //-------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        public SQLiteConnection conn;
        public bool IsSetFromCode = false;

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        private int MLocationIndex;
        private int MDesignLifeIndex;
        private int MImportanceClassIndex; // Clause A3—Building importance levels
        private float MAnnualProbabilityULS_Snow;
        private float MAnnualProbabilityULS_Wind;
        private float MAnnualProbabilityULS_EQ;
        private float MAnnualProbabilitySLS;
        private float MSiteElevation;
        private int MSnowRegionIndex;
        private int MExposureCategory;
        private int MWindRegionIndex;
        private int MTerrainCategoryIndex;
        private int MAngleWindDirectionIndex;
        private int MSiteSubSoilClassIndex;
        private float MFaultDistanceDmin;
        private float MFaultDistanceDmax;
        private float MZoneFactorZ;
        //private float MPeriodAlongXDirectionTx;
        //private float MPeriodAlongYDirectionTy;
        //private float MSpectralShapeFactorChTx;
       // private float MSpectralShapeFactorChTy;
        private float MAdditionalDeadActionRoof;
        private float MAdditionalDeadActionWall;
        private float MImposedActionRoof;

        private List<string> m_ListLocations;
        private List<string> m_ListDesignLife;
        private List<string> m_ListImportanceClass;
        private List<string> m_ListSnowRegion;
        private List<string> m_ListExposureCategory;
        private List<string> m_ListWindRegion;
        private List<string> m_ListTerrainCategory;
        private List<string> m_ListSiteSubSoilClass;

        // Not in GUI
        private float MDesignLife_Value;
        private float MR_ULS_Snow;
        private float MR_ULS_Wind;
        private float MR_ULS_EQ;
        private float MR_SLS;
        private ERoofExposureCategory MEExposureCategory;
        private EWindRegion MEWindRegion;
        private ESiteSubSoilClass MESiteSubSoilClass;

        //-------------------------------------------------------------------------------------------------------------
        public int LocationIndex
        {
            get
            {
                return MLocationIndex;
            }

            set
            {
                MLocationIndex = value;

                IsSetFromCode = true;

                SetLocationDependentDataFromDatabaseValues();

                IsSetFromCode = false;

                NotifyPropertyChanged("LocationIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int DesignLifeIndex
        {
            get
            {
                return MDesignLifeIndex;
            }
            set
            {
                MDesignLifeIndex = value;

                DesignLife_Value = CDatabaseManager.GetDesignLifeValueFromDatabase(MDesignLifeIndex);
                
                SetAnnualProbabilityValuesFromDatabaseValues();

                NotifyPropertyChanged("DesignLifeIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int ImportanceClassIndex
        {
            get
            {
                return MImportanceClassIndex;
            }

            set
            {
                MImportanceClassIndex = value;

                SetAnnualProbabilityValuesFromDatabaseValues();

                NotifyPropertyChanged("ImportanceClassIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float AnnualProbabilityULS_Wind
        {
            get
            {
                return MAnnualProbabilityULS_Wind;
            }

            set
            {
                MAnnualProbabilityULS_Wind = value;

                NotifyPropertyChanged("AnnualProbabilityULS_Wind");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float AnnualProbabilityULS_Snow
        {
            get
            {
                return MAnnualProbabilityULS_Snow;
            }

            set
            {
               MAnnualProbabilityULS_Snow = value;

               NotifyPropertyChanged("AnnualProbabilityULS_Snow");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float AnnualProbabilityULS_EQ
        {
            get
            {
                return MAnnualProbabilityULS_EQ;
            }

            set
            {
                MAnnualProbabilityULS_EQ = value;

                NotifyPropertyChanged("AnnualProbabilityULS_EQ");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float AnnualProbabilitySLS
        {
            get
            {
                return MAnnualProbabilitySLS;
            }

            set
            {
                MAnnualProbabilitySLS = value;

                NotifyPropertyChanged("AnnualProbabilitySLS");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SiteElevation
        {
            get
            {
                return MSiteElevation;
            }

            set
            {
                if (value < 0 || value > 2000)
                    throw new ArgumentException("Site elevation must be between 0 and 2000 meters");
                MSiteElevation = value;

                NotifyPropertyChanged("SiteElevation");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int SnowRegionIndex
        {
            get
            {
                return MSnowRegionIndex;
            }

            set
            {
                MSnowRegionIndex = value;

                NotifyPropertyChanged("SnowRegionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int ExposureCategoryIndex
        {
            get
            {
                return MExposureCategory;
            }

            set
            {
                MExposureCategory = value;

                MEExposureCategory = (ERoofExposureCategory)MExposureCategory;

                NotifyPropertyChanged("ExposureCategoryIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int WindRegionIndex
        {
            get
            {
                return MWindRegionIndex;
            }

            set
            {
                MWindRegionIndex = value;

                WindRegion = (EWindRegion)MWindRegionIndex;

                NotifyPropertyChanged("WindRegionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int TerrainCategoryIndex
        {
            get
            {
                return MTerrainCategoryIndex;
            }

            set
            {
                MTerrainCategoryIndex = value;

                NotifyPropertyChanged("TerrainCategoryIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int AngleWindDirectionIndex
        {
            get
            {
                return MAngleWindDirectionIndex;
            }

            set
            {
                MAngleWindDirectionIndex = value;

                NotifyPropertyChanged("AngleWindDirectionIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public int SiteSubSoilClassIndex
        {
            get
            {
                return MSiteSubSoilClassIndex;
            }

            set
            {
                if (value < 0 || value > 4)
                    throw new ArgumentException("Site subsoil class must be between A and E");
                MSiteSubSoilClassIndex = value;

                SiteSubSoilClass = (ESiteSubSoilClass)MSiteSubSoilClassIndex;

                //SetSpectralShapeFactorsFromDatabaseValues();

                NotifyPropertyChanged("SiteSubSoilClassIndex");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FaultDistanceDmin
        {
            get
            {
                return MFaultDistanceDmin;
            }

            set
            {
                MFaultDistanceDmin = value;

                NotifyPropertyChanged("FaultDistanceDmin");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float FaultDistanceDmax
        {
            get
            {
                return MFaultDistanceDmax;
            }

            set
            {
                MFaultDistanceDmax = value;

                NotifyPropertyChanged("FaultDistanceDmax");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float ZoneFactorZ
        {
            get
            {
                return MZoneFactorZ;
            }

            set
            {
                if (value < 0.01f || value > 0.90f)
                    throw new ArgumentException("Zone factor must be between 0.01 and 0.90");
                MZoneFactorZ = value;

                NotifyPropertyChanged("ZoneFactorZ");
            }
        }
        /*
        //-------------------------------------------------------------------------------------------------------------
        public float PeriodAlongXDirectionTx
        {
            get
            {
                return MPeriodAlongXDirectionTx;
            }

            set
            {
                if (value < 0.00f || value > 4.50f)
                    throw new ArgumentException("Period along X-direction Tx must be between 0.00 and 4.50 seconds");
                MPeriodAlongXDirectionTx = value;

                SetSpectralShapeFactorsFromDatabaseValues();

                NotifyPropertyChanged("PeriodAlongXDirectionTx");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float PeriodAlongYDirectionTy
        {
            get
            {
                return MPeriodAlongYDirectionTy;
            }

            set
            {
                if (value < 0.00f || value > 4.50f)
                    throw new ArgumentException("Period along Y-direction Ty must be between 0.00 and 4.50 seconds");
                MPeriodAlongYDirectionTy = value;

                SetSpectralShapeFactorsFromDatabaseValues();

                NotifyPropertyChanged("PeriodAlongYDirectionTy");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SpectralShapeFactorChTx
        {
            get
            {
                return MSpectralShapeFactorChTx;
            }

            set
            {
                if (value < 0.15f || value > 3.00f)
                    throw new ArgumentException("Spectral shape factor Ch T must be between 0.15 and 3.0");
                MSpectralShapeFactorChTx = value;

                NotifyPropertyChanged("SpectralShapeFactorChTx");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float SpectralShapeFactorChTy
        {
            get
            {
                return MSpectralShapeFactorChTy;
            }

            set
            {
                if (value < 0.15f || value > 3.00f)
                    throw new ArgumentException("Spectral shape factor Ch T must be between 0.15 and 3.0");
                MSpectralShapeFactorChTy = value;

                NotifyPropertyChanged("SpectralShapeFactorChTy");
            }
        }
        */

        //-------------------------------------------------------------------------------------------------------------
        public float AdditionalDeadActionRoof
        {
            get
            {
                return MAdditionalDeadActionRoof;
            }

            set
            {
                if (value < 0.0f || value > 10.00f)
                    throw new ArgumentException("Additional dead action value must be between 0.0 and 10.0 kN/m²");
                MAdditionalDeadActionRoof = value;

                NotifyPropertyChanged("AdditionalDeadActionRoof");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float AdditionalDeadActionWall
        {
            get
            {
                return MAdditionalDeadActionWall;
            }

            set
            {
                if (value < 0.0f || value > 10.00f)
                    throw new ArgumentException("Additional dead action value must be between 0.0 and 10.0 kN/m²");
                MAdditionalDeadActionWall = value;

                NotifyPropertyChanged("AdditionalDeadActionWall");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float ImposedActionRoof
        {
            get
            {
                return MImposedActionRoof;
            }

            set
            {
                if (value < 0.0f || value > 5.00f)
                    throw new ArgumentException("Imposed action value must be between 0.0 and 5.0 kN/m²");
                MImposedActionRoof = value;

                NotifyPropertyChanged("ImposedActionRoof");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float DesignLife_Value
        {
            get
            {
                return MDesignLife_Value;
            }

            set
            {
                MDesignLife_Value = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float R_ULS_Snow
        {
            get
            {
                return MR_ULS_Snow;
            }

            set
            {
                MR_ULS_Snow = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float R_ULS_Wind
        {
            get
            {
                return MR_ULS_Wind;
            }

            set
            {
                MR_ULS_Wind = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float R_ULS_EQ
        {
            get
            {
                return MR_ULS_EQ;
            }

            set
            {
                MR_ULS_EQ = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public float R_SLS
        {
            get
            {
                return MR_SLS;
            }

            set
            {
                MR_SLS = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public EWindRegion WindRegion
        {
            get
            {
                return MEWindRegion;
            }

            set
            {
                MEWindRegion = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public ESiteSubSoilClass SiteSubSoilClass
        {
            get
            {
                return MESiteSubSoilClass;
            }

            set
            {
                MESiteSubSoilClass = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public ERoofExposureCategory ExposureCategory
        {
            get
            {
                return MEExposureCategory;
            }

            set
            {
                MEExposureCategory = value;
            }
        }

        public List<string> ListLocations
        {
            get
            {
                return m_ListLocations;
            }

            set
            {
                m_ListLocations = value;
            }
        }

        public List<string> ListDesignLife
        {
            get
            {
                return m_ListDesignLife;
            }

            set
            {
                m_ListDesignLife = value;
            }
        }

        public List<string> ListImportanceClass
        {
            get
            {
                return m_ListImportanceClass;
            }

            set
            {
                m_ListImportanceClass = value;
            }
        }

        public List<string> ListSnowRegion
        {
            get
            {
                return m_ListSnowRegion;
            }

            set
            {
                m_ListSnowRegion = value;
            }
        }

        public List<string> ListExposureCategory
        {
            get
            {
                return m_ListExposureCategory;
            }

            set
            {
                m_ListExposureCategory = value;
            }
        }

        public List<string> ListWindRegion
        {
            get
            {
                return m_ListWindRegion;
            }

            set
            {
                m_ListWindRegion = value;
            }
        }

        public List<string> ListTerrainCategory
        {
            get
            {
                return m_ListTerrainCategory;
            }

            set
            {
                m_ListTerrainCategory = value;
            }
        }

        public List<string> ListSiteSubSoilClass
        {
            get
            {
                return m_ListSiteSubSoilClass;
            }

            set
            {
                m_ListSiteSubSoilClass = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public CPFDLoadInput(loadInputComboboxIndexes sloadInputComboBoxes, loadInputTextBoxValues sloadInputTextBoxes)
        {
            // Set default location
            LocationIndex = sloadInputComboBoxes.LocationIndex;
            DesignLifeIndex = sloadInputComboBoxes.DesignLifeIndex;
            ImportanceClassIndex = sloadInputComboBoxes.ImportanceLevelIndex;
            SiteSubSoilClassIndex = sloadInputComboBoxes.SiteSubSoilClassIndex;
            TerrainCategoryIndex = sloadInputComboBoxes.TerrainCategoryIndex;
            AngleWindDirectionIndex = sloadInputComboBoxes.AngleWindDirectionIndex;

            SiteElevation = sloadInputTextBoxes.SiteElevation;
            FaultDistanceDmin = sloadInputTextBoxes.FaultDistanceDmin_km;
            FaultDistanceDmax = sloadInputTextBoxes.FaultDistanceDmax_km;
            //PeriodAlongXDirectionTx = sloadInputTextBoxes.PeriodAlongXDirectionTx;
            //PeriodAlongYDirectionTy = sloadInputTextBoxes.PeriodAlongYDirectionTy;
            AdditionalDeadActionRoof = sloadInputTextBoxes.AdditionalDeadActionRoof;
            AdditionalDeadActionWall = sloadInputTextBoxes.AdditionalDeadActionWall;
            ImposedActionRoof = sloadInputTextBoxes.ImposedActionRoof;

            IsSetFromCode = false;
        }

        //-------------------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /*
        protected void SetSpectralShapeFactorsFromDatabaseValues()
        {
            List<float> sNaturalPeriod_T_Values = new List<float>();
            List<float> sFactor_Ch_ValuesForSpecificSoilClass = new List<float>();

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            // Connect to database
            using (conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["MainSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteDataReader reader = null;

                string sTableName = "SiteSubSoilClass";
                string sSiteSubSoilClass = "";

                // Set site soil class string value
                SQLiteCommand command = new SQLiteCommand("Select * from " + sTableName + " where ID = '" + SiteSubSoilClassIndex + "'", conn);

                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sSiteSubSoilClass = reader["class"].ToString();
                    }
                }

                sTableName = "ASNZS1170_5_Tab3_1_SSF";

                // Load all T and Ch values for the specific site subsoil class from the database
                command = new SQLiteCommand("Select * from ASNZS1170_5_Tab3_1_SSF", conn);
                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sNaturalPeriod_T_Values.Add(float.Parse(reader["periodT"].ToString(), nfi));
                        sFactor_Ch_ValuesForSpecificSoilClass.Add(float.Parse(reader[sSiteSubSoilClass].ToString(), nfi));
                    }
                }

                reader.Close();

                // Interpolate value - depends on natural period Tx, resp. Ty
                SpectralShapeFactorChTx = ArrayF.GetLinearInterpolationValuePositive(PeriodAlongXDirectionTx, sNaturalPeriod_T_Values.ToArray(), sFactor_Ch_ValuesForSpecificSoilClass.ToArray());
                SpectralShapeFactorChTy = ArrayF.GetLinearInterpolationValuePositive(PeriodAlongYDirectionTy, sNaturalPeriod_T_Values.ToArray(), sFactor_Ch_ValuesForSpecificSoilClass.ToArray());
            }
        }
        */
        protected void SetLocationDependentDataFromDatabaseValues()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            // Connect to database
            using (conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["MainSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteDataReader reader = null;
                string sTableName = "nzLocations";
                int cityID = LocationIndex;

                SQLiteCommand command = new SQLiteCommand("Select * from " + sTableName + " where ID = '" + cityID + "'", conn);

                using (reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Co tak v databaze dat spravny typ ???
                        // TO Ondrej - potrebujem najst conventor ktory to urobi automaticky a nebude tam pridavat ako prvy stlpec svoje ID
                        // Teraz pouzivam convertor ktory vsetko nastavi ako default na string

                        SnowRegionIndex = int.Parse(reader["snow_zone"].ToString()); //reader.GetInt32(reader.GetOrdinal("snow_zone"));
                        WindRegionIndex = int.Parse(reader["wind_zone"].ToString()); //reader.GetInt32(reader.GetOrdinal("wind_zone"));
                        WindRegion = (EWindRegion)WindRegionIndex;

                        int iMultiplier_M_lee_ID; // Could be empty
                        try
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("windMultiplierM_lee")))
                            {
                                iMultiplier_M_lee_ID = int.Parse(reader["windMultiplierM_lee"].ToString());

                                // TODO nacitat data pre index fMultiplier_M_lee_ID z databazy - tabulka multiplierM_lee, vzdialenost (zone_min a zone_max)
                            }
                        }
                        catch (ArgumentNullException) { iMultiplier_M_lee_ID = -1; }

                        int iRainZone = int.Parse(reader["rain_zone"].ToString());
                        int iCorrosionZone = int.Parse(reader["corrosion_zone"].ToString());

                        // Site elevation
                        SiteElevation = float.Parse(reader["E_average_m"].ToString());

                        // Earthquake
                        ZoneFactorZ = float.Parse(reader["eqFactorZ"].ToString(), nfi);

                        try
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("D_min_km")))
                            {
                                FaultDistanceDmin = float.Parse(reader["D_min_km"].ToString());
                            }
                        }
                        catch (ArgumentNullException) { FaultDistanceDmin = 9999.00f; }

                        try
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("D_max_km")))
                            {
                                FaultDistanceDmax = float.Parse(reader["D_max_km"].ToString());
                            }
                        }
                        catch (ArgumentNullException) { FaultDistanceDmax = 9999.00f; }
                    }
                }

                reader.Close();
            }

            //SetSpectralShapeFactorsFromDatabaseValues();
        }

        protected void SetAnnualProbabilityValuesFromDatabaseValues()
        {
            CAnnualProbability prob = CDatabaseManager.GetAnnualProbabilityValuesFromDatabase(DesignLifeIndex, ImportanceClassIndex);

            AnnualProbabilityULS_Wind = prob.AnnualProbabilityULS_Wind;
            AnnualProbabilityULS_Snow = prob.AnnualProbabilityULS_Snow;
            AnnualProbabilityULS_EQ = prob.AnnualProbabilityULS_EQ;
            AnnualProbabilitySLS = prob.AnnualProbabilitySLS;

            R_ULS_Wind = prob.R_ULS_Wind;
            R_ULS_Snow = prob.R_ULS_Snow;
            R_ULS_EQ = prob.R_ULS_EQ;
            R_SLS = prob.R_SLS;
        }
    }
}
