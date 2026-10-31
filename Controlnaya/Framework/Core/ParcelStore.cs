using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MyHttpServer.Parcels
{
    public record Parcel(int Id, string Recipient, int Weight, string Status);

    public enum OpStatus { Ok, BadRequest, NotFound }

    public record OpResult<T>(OpStatus Status, T Value, string Error);

    public record ParcelListing(IReadOnlyList<Parcel> Items, long TotalWeight);

    public class ParcelStore
    {
        public const string Registered = "registered";
        public const string Delivered = "delivered";

        private readonly object _lock = new();
        private readonly List<Parcel> _items = new();
        private int _nextId = 1;

        
        private static bool TryParseInt(string s, out int value) =>
            int.TryParse(s?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value);

        private static string Validate(string recipient, string weight, out string name, out int w)
        {
            name = (recipient ?? "").Trim();
            w = 0;
            if (!TryParseInt(weight, out w) || w < 1 || w > 20000)
                return "Вес должен быть целым числом от 1 до 20000 граммов.";
            return null;
        }

        public OpResult<ParcelListing> GetList(string minWeight)
        {
            int min = 0;
            if (!string.IsNullOrWhiteSpace(minWeight) && !TryParseInt(minWeight, out min))
                return new(OpStatus.BadRequest, null, "minWeight должен быть неотрицательным целым числом.");

            lock (_lock)
            {
                var items = _items.Where(p => p.Weight >= min).OrderBy(p => p.Id).ToList();
                long total = items.Sum(p => (long)p.Weight);
                return new(OpStatus.Ok, new ParcelListing(items, total), null);
            }
        }

        public OpResult<Parcel> GetById(string id)
        {
            if (!TryParseInt(id, out int idValue))
                return new(OpStatus.BadRequest, null, "Некорректный id.");
            lock (_lock)
            {
                var p = _items.FirstOrDefault(x => x.Id == idValue);
                return p == null
                    ? new(OpStatus.NotFound, null, "Посылка не найдена.")
                    : new(OpStatus.Ok, p, null);
            }
        }

        public OpResult<Parcel> Create(string recipient, string weight)
        {
            string error = Validate(recipient, weight, out string name, out int w);
            if (error != null) return new(OpStatus.BadRequest, null, error);

            lock (_lock)
            {
                var p = new Parcel(_nextId++, name, w, Registered);
                _items.Add(p);
                return new(OpStatus.Ok, p, null);
            }
        }

        public OpResult<Parcel> Update(string id, string recipient, string weight, string status)
        {
            if (!TryParseInt(id, out int idValue))
                return new(OpStatus.BadRequest, null, "Некорректный id.");

            string error = Validate(recipient, weight, out string name, out int w);
            if (error == null && status != Registered && status != Delivered)
                error = "Статус должен быть registered или delivered.";
            if (error != null) return new(OpStatus.BadRequest, null, error);

            lock (_lock)
            {
                int index = _items.FindIndex(x => x.Id == idValue);
                if (index < 0) return new(OpStatus.NotFound, null, "Посылка не найдена.");

                var updated = new Parcel(idValue, name, w, status);
                _items[index] = updated;
                return new(OpStatus.Ok, updated, null);
            }
        }
    }
}
