﻿using UnityEngine;

namespace GGS_Framework
{
	[System.Serializable]
    public class IntRange
    {
        #region Class Members
        public int start;
        public int end;
        #endregion

        #region Class Accesors
        public int Lenght
        {
            get { return end - start; }
        }
        #endregion

        #region Class Implementation
        public IntRange ()
        {
            start = 0;
            end = 0;
        }

        public IntRange (int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public bool InRange (int value)
        {
            return (value >= start && value <= end);
        }

        public int GetRandomValue ()
        {
            return Random.Range (start, end);
        }
        #endregion
    }
}