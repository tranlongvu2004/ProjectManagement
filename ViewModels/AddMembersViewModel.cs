// ViewModels/AddMembersViewModel.cs
using PorjectManagement.Models;
using System;
using System.Collections.Generic;

namespace PorjectManagement.ViewModels
{
    public class UserListItemVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string RoleName { get; set; } = "";
        public int RoleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ProgressPercent { get; set; }
        public string Status { get; set; } = ""; // "Đang thực tập", "Hoàn thành", ...
    }

    public class AddMembersViewModel
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public List<UserListItemVM>? Users { get; set; }
        public List<int>? SelectedUserIds { get; set; } 

        public List<Project>? AllProjects { get; set; }  
       

    }

}
