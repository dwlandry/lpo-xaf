﻿using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using LPO.Module.BusinessObjects.Projects;
using LPO.Module.BusinessObjects.Documents;
using LPO.Module.BusinessObjects.Instrument_Spec;
using DevExpress.CodeParser;

namespace LPO.Module.BusinessObjects.Instruments
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty("Tag")]
    [DefaultListViewOptions(MasterDetailMode.ListViewOnly, true, NewItemRowPosition.Bottom)]
    [Persistent("inst_instrument")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class Instrument : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public Instrument(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }
        protected override void OnSaving()
        {
            if (instrumentTypeChanged)
                AddInstrumentSpecItems();
            base.OnSaving();
        }
        protected override void OnSaved()
        {
            
            base.OnSaved();
        }
        void AddInstrumentSpecItems()
        {
            // If there are existing InstrumentSpecItems, set their IsActive to false.
            for (int i = 0; i < InstrumentSpecItems.Count; i++)
            {
                InstrumentSpecItems[i].IsActive = false;
            }

            XPQuery<InstrumentSpecItem> instSpecItems = new XPQuery<InstrumentSpecItem>(base.Session);
            var instrumentSpecItemsForThisInstrument = from i in instSpecItems where i.Instrument.Oid == Oid select i.SpecItem.Oid;

            foreach (var item in InstrumentType.SpecItems)
            {
                if (instrumentSpecItemsForThisInstrument.Contains(item.Oid))
                {
                    var obj = from i in InstrumentSpecItems
                              where i.SpecItem.Oid == item.Oid
                              select i;

                    obj.ToList()[0].IsActive = true;
                }
                else
                {
                    InstrumentSpecItem instrumentSpecItem = new InstrumentSpecItem(Session)
                    {
                        SpecItem = item,
                        IsActive = true,
                        RefInstrumentType = instrumentType
                    };
                    InstrumentSpecItems.Add(instrumentSpecItem);
                }
            }

        }

        
        Project project;
        [Association("Project-Instruments")]
        public Project Project
        {
            get => project;
            set => SetPropertyValue(nameof(Project), ref project, value);
        }

        string tagPrefix;
        [Size(10)]
        [XafDisplayName("Prefix")]
        [Persistent(@"tag_prefix")]
        public string TagPrefix
        {
            get => tagPrefix;
            set => SetPropertyValue(nameof(TagPrefix), ref tagPrefix, value);
        }
        string tagLetters;
        [Size(10)]
        [XafDisplayName("Letters")]
        [Persistent(@"tag_letters")]
        public string TagLetters
        {
            get => tagLetters;
            set => SetPropertyValue(nameof(TagLetters), ref tagLetters, value);
        }
        string tagNumbers;
        [Size(10)]
        [XafDisplayName("Numbers")]
        [Persistent(@"tag_numbers")]
        public string TagNumbers
        {
            get => tagNumbers;
            set => SetPropertyValue(nameof(TagNumbers), ref tagNumbers, value);
        }
        string tagSuffix;
        [Size(10)]
        [XafDisplayName("Suffix")]
        [Persistent(@"tag_suffix")]
        public string TagSuffix
        {
            get => tagSuffix;
            set => SetPropertyValue(nameof(TagSuffix), ref tagSuffix, value);
        }

        public string Tag { get => String.Format("{0}{1}-{2}{3}", TagPrefix, TagLetters, TagNumbers, TagSuffix); }

        bool instrumentTypeChanged;
        InstrumentType instrumentType;
        [Persistent(@"instrument_type")]
        public InstrumentType InstrumentType
        {
            get => instrumentType;
            set
            {
                if (instrumentType == value)
                {
                    return;
                }

                instrumentTypeChanged = true;
                SetPropertyValue(nameof(InstrumentType), ref instrumentType, value);
                RaisePropertyChangedEvent(nameof(InstrumentType));
            }
        }

        InstrumentStatus status;
        public InstrumentStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        ProjectScope projectScope;
        public ProjectScope ProjectScope
        {
            get => projectScope;
            set => SetPropertyValue(nameof(ProjectScope), ref projectScope, value);
        }

        PID pID;
        [Association("PID-Instruments")]
        public PID PID
        {
            get => pID;
            set => SetPropertyValue(nameof(PID), ref pID, value);
        }

        [EditorAlias("HyperLinkStringPropertyEditor")]
        [XafDisplayName("PID Link")]
        public string PathToPID => PID != null && PID.File != null ? PID.File.RealFileName : string.Empty;

        string processDescription;
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [Persistent(@"process_description")]
        public string ProcessDescription
        {
            get => processDescription;
            set => SetPropertyValue(nameof(ProcessDescription), ref processDescription, value);
        }
        string comments;
        [Size(255)]
        public string Comments
        {
            get => comments;
            set => SetPropertyValue(nameof(Comments), ref comments, value);
        }

        [Association("Instrument-InstrumentSpecItems")]
        public XPCollection<InstrumentSpecItem> InstrumentSpecItems => GetCollection<InstrumentSpecItem>(nameof(InstrumentSpecItems));
    }
}