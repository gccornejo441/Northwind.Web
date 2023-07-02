namespace BlazinService.Data
{
    public class UserService
    {
        public List<User> userList = new List<User>()
        {
            new User(){ Id = 1, UserName = "gcornejo", Password = "password123"},
            new User(){ Id = 2, UserName = "mlitner", Password = "password123"},

        };

        public Task<User> GetUserAsync(int id)
        {
            return Task.FromResult(userList[id]);
        }

    }
}
