using System.Collections.Generic;
using Smartbot.Utilities.Handlers._4sq.FourSquareServices.Entities;

namespace Smartbot.Utilities.Handlers._4sq.FourSquareServices.Core
{
    public class VenueExploreResponseContainer
    {
        public VenueExploreResponse response;
    }

    public class VenueExploreResponse
    {
        public int suggestedRadius
        {
            get;
            set;
        }

        public string headerLocation
        {
            get;
            set;
        }

        public string headerFullLocation
        {
            get;
            set;
        }

        public string headerLocationGranularity
        {
            get;
            set;
        }

        public int totalResults
        {
            get;
            set;
        }

        public List<FourSquareEntityItems<VenueExplore>> groups
        {
            get;
            set;
        }
    }
}