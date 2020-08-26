using System;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Scouts.Dev;
using Scouts.Models.Enums;
using Xamarin.Forms;

namespace Scouts.Models
{
    [BsonIgnoreExtraElements]
    public class InfoModel
    {
        [BsonId] public ObjectId id { get; set; } = ObjectId.GenerateNewId();
        public string Title { get; set; }
        public string Summary { get; set; }

        public DateTime PostedTime;

        public TargetPublicType InfoPublicType;
        public FileType InfoAttachType;
        public EventType InfoEventType;

        public bool IsUrgent;

        [BsonIgnore] public ImageSource Image { get; set; }

        [BsonIgnore]
        public string PostedTimeString =>
            PostedTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm", CultureInfo.CreateSpecificCulture("fr-FR"));

        [BsonIgnore] public Color InfoBackColor => Helpers.InfoModelColors[(int) InfoPublicType];

        [BsonIgnore] public Color InfoTextColor
        {
            get
            {
                var nThreshold = 105;
                var bgDelta = Convert.ToInt32(InfoBackColor.R * 255 * 0.299 + InfoBackColor.G * 255 * 0.587 +
                                              InfoBackColor.B * 255 * 0.114);

                var foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
                return foreColor;
            }
        }
    }
}