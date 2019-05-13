﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using RipAndBurn.Rip.CDMetadata;
//
//    var trackList = TrackList.FromJson(jsonString);

namespace RipAndBurn.Rip.CDMetadata.Album {
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class TrackList {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("text-representation", NullValueHandling = NullValueHandling.Ignore)]
        public TextRepresentation TextRepresentation { get; set; }

        [JsonProperty("status-id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? StatusId { get; set; }

        [JsonProperty("artist-credit", NullValueHandling = NullValueHandling.Ignore)]
        public List<ArtistCredit> ArtistCredit { get; set; }

        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Date { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("quality", NullValueHandling = NullValueHandling.Ignore)]
        public string Quality { get; set; }

        [JsonProperty("disambiguation", NullValueHandling = NullValueHandling.Ignore)]
        public string Disambiguation { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty("packaging", NullValueHandling = NullValueHandling.Ignore)]
        public string Packaging { get; set; }

        [JsonProperty("release-events", NullValueHandling = NullValueHandling.Ignore)]
        public List<ReleaseEvent> ReleaseEvents { get; set; }

        [JsonProperty("packaging-id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? PackagingId { get; set; }

        [JsonProperty("cover-art-archive", NullValueHandling = NullValueHandling.Ignore)]
        public CoverArtArchive CoverArtArchive { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("asin")]
        public object Asin { get; set; }

        [JsonProperty("barcode", NullValueHandling = NullValueHandling.Ignore)]
        public string Barcode { get; set; }

        [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
        public List<Media> Media { get; set; }
    }

    public partial class ArtistCredit {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("artist", NullValueHandling = NullValueHandling.Ignore)]
        public Artist Artist { get; set; }

        [JsonProperty("joinphrase", NullValueHandling = NullValueHandling.Ignore)]
        public string Joinphrase { get; set; }
    }

    public partial class Artist {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("disambiguation", NullValueHandling = NullValueHandling.Ignore)]
        public string Disambiguation { get; set; }

        [JsonProperty("sort-name", NullValueHandling = NullValueHandling.Ignore)]
        public string SortName { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("iso-3166-1-codes", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Iso31661Codes { get; set; }
    }

    public partial class CoverArtArchive {
        [JsonProperty("artwork", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Artwork { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("darkened", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Darkened { get; set; }

        [JsonProperty("back", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Back { get; set; }

        [JsonProperty("front", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Front { get; set; }
    }

    public partial class Media {
        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("tracks", NullValueHandling = NullValueHandling.Ignore)]
        public List<Track> Tracks { get; set; }

        [JsonProperty("track-count", NullValueHandling = NullValueHandling.Ignore)]
        public long? TrackCount { get; set; }

        [JsonProperty("format-id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? FormatId { get; set; }

        [JsonProperty("track-offset", NullValueHandling = NullValueHandling.Ignore)]
        public long? TrackOffset { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("data-tracks", NullValueHandling = NullValueHandling.Ignore)]
        public List<Track> DataTracks { get; set; }
    }

    public partial class Track {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("artist-credit", NullValueHandling = NullValueHandling.Ignore)]
        public List<ArtistCredit> ArtistCredit { get; set; }

        [JsonProperty("recording", NullValueHandling = NullValueHandling.Ignore)]
        public Recording Recording { get; set; }

        [JsonProperty("length")]
        public long? Length { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Number { get; set; }
    }

    public partial class Recording {
        [JsonProperty("length")]
        public long? Length { get; set; }

        [JsonProperty("artist-credit", NullValueHandling = NullValueHandling.Ignore)]
        public List<ArtistCredit> ArtistCredit { get; set; }

        [JsonProperty("disambiguation", NullValueHandling = NullValueHandling.Ignore)]
        public string Disambiguation { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Video { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }
    }

    public partial class ReleaseEvent {
        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Date { get; set; }

        [JsonProperty("area", NullValueHandling = NullValueHandling.Ignore)]
        public Artist Area { get; set; }
    }

    public partial class TextRepresentation {
        [JsonProperty("script", NullValueHandling = NullValueHandling.Ignore)]
        public string Script { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }
    }

    public enum Name { Canada, TheDoors, UnitedStates };

    public enum SortName { Canada, DoorsThe, UnitedStates };

    public partial class TrackList {
        public static TrackList FromJson(string json) => JsonConvert.DeserializeObject<TrackList>(json, Converter.Settings);
    }

    public static class Serialize {
        public static string ToJson(this TrackList self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NameConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value) {
                case "Canada":
                    return Name.Canada;
                case "The Doors":
                    return Name.TheDoors;
                case "United States":
                    return Name.UnitedStates;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value) {
                case Name.Canada:
                    serializer.Serialize(writer, "Canada");
                    return;
                case Name.TheDoors:
                    serializer.Serialize(writer, "The Doors");
                    return;
                case Name.UnitedStates:
                    serializer.Serialize(writer, "United States");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }

    internal class SortNameConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(SortName) || t == typeof(SortName?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value) {
                case "Canada":
                    return SortName.Canada;
                case "Doors, The":
                    return SortName.DoorsThe;
                case "United States":
                    return SortName.UnitedStates;
            }
            throw new Exception("Cannot unmarshal type SortName");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SortName)untypedValue;
            switch (value) {
                case SortName.Canada:
                    serializer.Serialize(writer, "Canada");
                    return;
                case SortName.DoorsThe:
                    serializer.Serialize(writer, "Doors, The");
                    return;
                case SortName.UnitedStates:
                    serializer.Serialize(writer, "United States");
                    return;
            }
            throw new Exception("Cannot marshal type SortName");
        }

        public static readonly SortNameConverter Singleton = new SortNameConverter();
    }

    internal class ParseStringConverter : JsonConverter {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l)) {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer) {
            if (untypedValue == null) {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}

