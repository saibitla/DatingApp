namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRespository UserRepository {get;}

        IMessageRepository MessageRepository {get;}

        ILikesRepository LikesRepository {get;}

        Task<bool> Complete();

        bool HasChanges();
    }
}