using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.DAL.Entities
{
	public class Permission:User
	{
		public string Controller { get; set; }//UserManagement  //ExamManagement
		public string Name { get; set; } //CreateExam  //EditExam  //DeleteExam  //ViewExam
	}
}
