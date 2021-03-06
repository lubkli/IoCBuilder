﻿namespace IoCBuilder
{
    /// <summary>
    /// Indicates the search mode to be used for locator queries
    /// </summary>
    public enum SearchMode
    {
        /// <summary>
        /// Search local first and then up the containment heirarchy as needed
        /// </summary>
        Up,

        /// <summary>
        /// Search local only
        /// </summary>
        Local
    }
}