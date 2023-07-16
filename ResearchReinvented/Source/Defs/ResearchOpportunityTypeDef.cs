using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
	[Flags]
	public enum HandlingMode 
	{
		Job_Theory = 1,
        Job_Analysis = 1 << 1,
		Social = 1 << 2,
        Special_OnIngest = 1 << 3,
        Special_OnIngest_Observable = 1 << 4,
		Special_Medicine = 1 << 5,
        Special_Prototype = 1 << 6,
        Special_Tooling = 1 << 7,
    }

    public class ResearchOpportunityTypeDef : Def
    {
		public HandlingMode handledBy;

		public Type jobPickerClass;
		public JobDef jobDef;

		public string header_Direct;
        public string header_Ancestor;
        public string header_Descendant;

		[Unsaved(false)]
		protected TaggedString cachedHeader_DirectCap = null;
		[Unsaved(false)]
		protected TaggedString cachedHeader_AncestorCap = null;
		[Unsaved(false)]
		protected TaggedString cachedHeader_DescendantCap = null;

		public string shortDesc_Direct;
		public string shortDesc_Ancestor;
		public string shortDesc_Descendant;

		[Unsaved(false)]
		protected TaggedString cachedShortDesc_DirectCap = null;
		[Unsaved(false)]
		protected TaggedString cachedShortDesc_AncestorCap = null;
		[Unsaved(false)]
		protected TaggedString cachedShortDesc_DescendantCap = null;

		public ResearchOpportunityCategoryDef category_Direct;
		public ResearchOpportunityCategoryDef category_Ancestor;
		public ResearchOpportunityCategoryDef category_Descendant;

		public List<string> hintIcons;
		private List<Texture2D> _icons;
        public List<Texture2D> Icons
        {
            get
            {
                if (_icons == null)
                {
					_icons = new List<Texture2D>();
					if(hintIcons != null)
                    {
                        foreach (var path in hintIcons)
                            _icons.Add(ContentFinder<Texture2D>.Get(path, true));
                    }
                }
				return _icons;
            }
        }

        //public Dictionary<HandlingMode, float> handlingModeModifiers = new Dictionary<HandlingMode, float>();
        public bool generatesPopups = false;

        public TaggedString Header_DirectCap
		{
			get
			{
				if (this.header_Direct.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedHeader_DirectCap.NullOrEmpty())
				{
					this.cachedHeader_DirectCap = this.header_Direct.CapitalizeFirst();
				}
				return this.cachedHeader_DirectCap;
			}
		}

		public TaggedString Header_AncestorCap
		{
			get
			{
				if (this.header_Ancestor.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedHeader_AncestorCap.NullOrEmpty())
				{
					this.cachedHeader_AncestorCap = this.header_Ancestor.CapitalizeFirst();
				}
				return this.cachedHeader_AncestorCap;
			}
		}

		public TaggedString Header_DescendantCap
		{
			get
			{
				if (this.header_Descendant.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedHeader_DescendantCap.NullOrEmpty())
				{
					this.cachedHeader_DescendantCap = this.header_Descendant.CapitalizeFirst();
				}
				return this.cachedHeader_DescendantCap;
			}
		}

		public TaggedString ShortDesc_DirectCap
		{
			get
			{
				if (this.shortDesc_Direct.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedShortDesc_DirectCap.NullOrEmpty())
				{
					this.cachedShortDesc_DirectCap = this.shortDesc_Direct.CapitalizeFirst();
				}
				return this.cachedShortDesc_DirectCap;
			}
		}

		public TaggedString ShortDesc_AncestorCap
		{
			get
			{
				if (this.shortDesc_Ancestor.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedShortDesc_AncestorCap.NullOrEmpty())
				{
					this.cachedShortDesc_AncestorCap = this.shortDesc_Ancestor.CapitalizeFirst();
				}
				return this.cachedShortDesc_AncestorCap;
			}
		}

		public TaggedString ShortDesc_DescendantCap
		{
			get
			{
				if (this.shortDesc_Descendant.NullOrEmpty())
				{
					return null;
				}
				if (this.cachedShortDesc_DescendantCap.NullOrEmpty())
				{
					this.cachedShortDesc_DescendantCap = this.shortDesc_Descendant.CapitalizeFirst();
				}
				return this.cachedShortDesc_DescendantCap;
			}
		}

        public bool UsesChunkedResearch
        {
            get
            {
				return handledBy.HasFlag(HandlingMode.Special_OnIngest) || handledBy.HasFlag(HandlingMode.Special_Prototype);
            }
        }

        public ResearchOpportunityCategoryDef GetCategory(ResearchRelation relation) 
		{
			switch (relation)
			{
				case ResearchRelation.Ancestor: return this.category_Ancestor != null ? this.category_Ancestor : this.category_Direct;
				case ResearchRelation.Descendant: return this.category_Descendant != null ? this.category_Descendant : this.category_Direct;
				case ResearchRelation.Direct: default: return this.category_Direct;
			}
		}

		public IEnumerable<ResearchOpportunityCategoryDef> GetAllCategories()
		{
			HashSet<ResearchOpportunityCategoryDef> categories = new HashSet<ResearchOpportunityCategoryDef>();
			if(this.category_Direct != null)
				categories.Add(this.category_Direct);
			if (this.category_Ancestor != null)
				categories.Add(this.category_Ancestor);
			if (this.category_Descendant != null)
				categories.Add(this.category_Descendant);
			return categories;
		}


		public TaggedString GetHeaderCap(ResearchRelation relation) 
		{
            switch (relation)
            {
				case ResearchRelation.Ancestor: return this.Header_AncestorCap != null ? this.Header_AncestorCap : this.Header_DirectCap;
				case ResearchRelation.Descendant: return this.Header_DescendantCap != null ? this.Header_DescendantCap : this.Header_DirectCap;
				case ResearchRelation.Direct: default: return this.Header_DirectCap;
			}
		}

		public TaggedString GetShortDescCap(ResearchRelation relation)
		{
			switch (relation)
			{
				case ResearchRelation.Ancestor: return this.ShortDesc_AncestorCap != null ? this.ShortDesc_AncestorCap : this.ShortDesc_DirectCap;
				case ResearchRelation.Descendant: return this.ShortDesc_DescendantCap != null ? this.ShortDesc_DescendantCap : this.ShortDesc_DirectCap;
				case ResearchRelation.Direct: default: return this.ShortDesc_DirectCap;
			}
		}
    }
}
