using System.Collections.Concurrent;

namespace ChatHub.Api.Services;

public class UserPresenceTracker
{
    private readonly ConcurrentDictionary<int, int>
        _connections = new();

    /* =========================================================
       Connect
       ========================================================= */

    public bool Connect(int userId)
    {
        if (userId <= 0)
            return false;

        var count =
            _connections.AddOrUpdate(
                userId,
                1,
                (_, current) => current + 1);

        /*
         * اگر Connection قبلی وجود نداشته،
         * کاربر الان Online شده است.
         */
        return count == 1;
    }

    /* =========================================================
       Disconnect
       ========================================================= */

    public bool Disconnect(int userId)
    {
        if (userId <= 0)
            return false;

        while (true)
        {
            if (!_connections.TryGetValue(
                    userId,
                    out var currentCount))
            {
                return false;
            }

            if (currentCount <= 1)
            {
                if (_connections.TryRemove(
                        userId,
                        out _))
                {
                    /*
                     * آخرین Connection بسته شد.
                     * کاربر واقعاً Offline شده است.
                     */
                    return true;
                }

                continue;
            }

            if (_connections.TryUpdate(
                    userId,
                    currentCount - 1,
                    currentCount))
            {
                /*
                 * هنوز حداقل یک Connection باقی مانده.
                 */
                return false;
            }
        }
    }

    /* =========================================================
       Is Online
       ========================================================= */

    public bool IsOnline(int userId)
    {
        if (userId <= 0)
            return false;

        return _connections.ContainsKey(userId);
    }

    /* =========================================================
       Get Online User Ids
       ========================================================= */

    public List<int> GetOnlineUserIds()
    {
        return _connections.Keys.ToList();
    }
}