using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Storage;

namespace GameProject
{
    public struct HighscoreData
    {
        #region Fields

        // information support
        public string[] PlayerNames;
        public int[] Scores;
        public int Count;
        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor to store highscore data
        /// </summary>
        public HighscoreData(int count)
        {
            PlayerNames = new string[count];
            Scores = new int[count];

            Count = count; 
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Save highscores
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <param name="device"></param>
        public static void SaveHighScores(HighscoreData data, string filename)
        {
            // Get the path of the save game
            string fullpath = "highscores.dat";

            // Open the file, creating it if necessary
            FileStream stream = File.Open(fullpath, FileMode.OpenOrCreate);
            try
            {
                // Convert the object to XML data and put it in the stream
                XmlSerializer serializer = new XmlSerializer(typeof(HighscoreData));
                serializer.Serialize(stream, data);
            }
            finally
            {
                // Close the file
                stream.Close();
            }
            
        }

        /// <summary>
        /// Load highscores.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static HighscoreData LoadHighScores(string filename)
        {
            HighscoreData data;

            // Get the path of the save game
            string fullpath = "highscores.dat";

            // Open the file
            FileStream stream = File.Open(fullpath, FileMode.OpenOrCreate, FileAccess.Read);
            try
            {
                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(HighscoreData));
                data = (HighscoreData)serializer.Deserialize(stream);
            }
            finally
            {
                // Close the file
                stream.Close();
            }
           

            return (data);
        }

        #endregion

    }
}
