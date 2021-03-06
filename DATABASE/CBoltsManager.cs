﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using DATABASE.DTO;

namespace DATABASE
{
    public static class CBoltsManager
    {
        public static Dictionary<string, CBoltProperties> DictBoltProperties;

        public static List<CBoltProperties> LoadBoltsProperties(string TableName)
        {
            CBoltProperties properties = null;
            List<CBoltProperties> items = new List<CBoltProperties>();

            using (SQLiteConnection conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["BoltsSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from "+TableName, conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties = GetBoltProperties(reader);

                        items.Add(properties);
                    }
                }
            }
            return items;
        }

        private static void LoadBoltsPropertiesDictionary(string TableName)
        {
            DictBoltProperties = new Dictionary<string, CBoltProperties>();
            
            using (SQLiteConnection conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["BoltsSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from " + TableName, conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CBoltProperties properties = GetBoltProperties(reader);
                        DictBoltProperties.Add(properties.Name, properties);
                    }
                }
            }
        }

        public static CBoltProperties GetBoltProperties(string name, string TableName)
        {
            if (DictBoltProperties == null) LoadBoltsPropertiesDictionary(TableName);

            CBoltProperties prop = null;
            DictBoltProperties.TryGetValue(name, out prop);

            return prop;
        }

        public static CBoltProperties GetBoltProperties(int id, string TableName)
        {
            CBoltProperties properties = null;
            using (SQLiteConnection conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["BoltsSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from" + TableName +" WHERE ID = @id", conn);
                command.Parameters.AddWithValue("@id", id);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        properties = GetBoltProperties(reader);
                    }
                }
            }
            return properties;
        }

        private static CBoltProperties GetBoltProperties(SQLiteDataReader reader)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            CBoltProperties properties = new CBoltProperties();
            properties.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            properties.Name = reader["Name"].ToString();
            properties.Standard = reader["Standard"].ToString();
            properties.ThreadDiameter = reader["threadDiameter"].ToString() == "" ? double.NaN : double.Parse(reader["threadDiameter"].ToString(), nfi) / 1000f;
            properties.ShankDiameter = reader["shankDiameter"].ToString() == "" ? double.NaN : double.Parse(reader["shankDiameter"].ToString(), nfi) / 1000f;
            properties.PitchDiameter = reader["pitchDiameter"].ToString() == "" ? double.NaN : double.Parse(reader["pitchDiameter"].ToString(), nfi) / 1000f;
            properties.Pitch_coarse = reader["pitch_coarse"].ToString() == "" ? double.NaN : double.Parse(reader["pitch_coarse"].ToString(), nfi) / 1000f;
            properties.Pitch_fine = reader["pitch_fine"].ToString() == "" ? double.NaN : double.Parse(reader["pitch_fine"].ToString(), nfi) / 1000f;
            properties.Code = reader["code"].ToString();
            properties.ThreadAngle_deg = reader["threadAngle"].ToString() == "" ? double.NaN : double.Parse(reader["threadAngle"].ToString(), nfi);
            properties.H = reader["H"].ToString() == "" ? double.NaN : double.Parse(reader["H"].ToString(), nfi) / 1000f;

            properties.Mass_kg_LM = reader["mass_kg_LM"].ToString() == "" ? double.NaN : double.Parse(reader["mass_kg_LM"].ToString(), nfi);
            properties.Price_PPLM_NZD = reader["price_PPLM_NZD"].ToString() == "" ? double.NaN : double.Parse(reader["price_PPLM_NZD"].ToString(), nfi);
            properties.Price_PPKG_NZD = reader["price_PPKG_NZD"].ToString() == "" ? double.NaN : double.Parse(reader["price_PPKG_NZD"].ToString(), nfi);

            return properties;
        }
    }
}