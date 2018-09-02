﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using DATABASE.DTO;
using CRSC;
using MATERIAL;

namespace DATABASE
{
    public static class CTEKScrewsManager
    {
        public static List<CTEKScrewProperties> LoadTEKScrewsProperties()
        {
            CTEKScrewProperties properties;
            List<CTEKScrewProperties> items = new List<CTEKScrewProperties>();

            using (SQLiteConnection conn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["TEKScrewsSQLiteDB"].ConnectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand("Select * from TEKScrews", conn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties = new CTEKScrewProperties();
                        properties.ID = reader.GetInt32(reader.GetOrdinal("ID"));
                        properties.gauge = reader["gauge"].ToString();
                        properties.threadDiameter = reader["threadDiameter"].ToString();
                        properties.shankDiameter = reader["shankDiameter"].ToString();
                        properties.threadType1 = reader["threadType1"].ToString();
                        properties.threadsPerInch1 = reader["threadsPerInch1"].ToString();
                        properties.threadType2 = reader["threadType2"].ToString();
                        properties.threadsPerInch2 = reader["threadsPerInch2"].ToString();
                        properties.threadType3 = reader["threadType3"].ToString();
                        properties.threadsPerInch3 = reader["threadsPerInch3"].ToString();
                        properties.headSizeInch = reader["headSizeInch"].ToString();
                        properties.headSizemm = reader["headSizemm"].ToString();
                        properties.washerSizemm = reader["washerSizemm"].ToString();
                        properties.washerThicknessmm = reader["washerThicknessmm"].ToString();

                        items.Add(properties);
                    }
                }
            }
            return items;
        }
    }
}