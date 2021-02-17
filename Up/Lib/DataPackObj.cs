using System.Collections.Generic;

namespace Lib
{
    public enum DataType
    {
        CheckLogin, Login, 
        GetTrashGroups, GetTrashGroupInfo, UpdataTrash,
        AddTrashGroup, MoveTrashGroup, RenameTrashGroup,
        SetTrashNick, CheckTrashUUID,
        GetUserGroups, GetUserGroupInfo,
        GetUserGroup, GetUserTask,
        AddUserGroup, MoveUserGroup, RenameUserGroup,
        AddUser, DeleteUser, SetUser, SetUserTask,
        GetUserGroupBind, SetUserGroupBind,
        UpdataNow
    }
    public enum ItemState
    {
        正常, 初始化, 初始化完成, 距离传感器错误, Null, 快满了
    }
    public class DataPackObj
    {
        public string Token { get; set; }
        public DataType Type { get; set; }
        public bool Res { get; set; }
        public string Data { get; set; }
        public string Data1 { get; set; }
    }

    public class TrashDataSaveObj
    {
        public Dictionary<string, TrashSaveObj> List { get; set; }
        public string Name { get; set; }
    }
    public class TrashSaveObj
    {
        public string UUID { get; set; }
        public string Nick { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Capacity { get; set; }
        public string Time { get; set; }
        public bool Open { get; set; }
        public ItemState State { get; set; }
        public string SIM { get; set; }
        public int Battery { get; set; }
    }

    public class UserSaveObj
    {
        public string ID { get; set; }
        public string Group { get; set; }
        public string Pass { get; set; }
        public string LoginTime { get; set; }
    }
    public class UserDataSaveObj
    {
        public Dictionary<string, UserSaveObj> List { get; set; }
        public List<string> Bind { get; set; }
        public string Name { get; set; }
    }
}
