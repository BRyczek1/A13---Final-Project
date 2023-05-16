using MovieLibraryEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLibraryOO.Services
{
    public interface IUserService
    {
        public User LoggedInAs { get; set; }
    }
}
