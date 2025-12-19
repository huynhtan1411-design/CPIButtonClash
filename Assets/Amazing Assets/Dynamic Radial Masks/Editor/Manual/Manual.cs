// Dynamic Radial Masks <https://u3d.as/1w0H>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System;

using UnityEngine;


namespace AmazingAssets.DynamicRadialMasks.Editor.Manual
{
    public class Manual : ScriptableObject
    {
        public enum URLType { OpenPage, MailTo }


        public bool showHeader;
        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;

        [Serializable]
        public class Section
        {
            public string heading, text, linkText, url;
            public URLType urlType;
        }
    }
}
 
