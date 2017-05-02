﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AskAbout.Models;
using AskAbout.Data;
using AskAbout.Models.QuestionViewModels;
using AskAbout.Services;
using AskAbout.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AskAbout.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IQuestionServices _questionServices;
        private readonly UserManager<User> _userManager;

        public QuestionController(
            ApplicationDbContext context,
            IQuestionServices questionServices,
            UserManager<User> userManager)
        {
            _db = context;
            _questionServices = questionServices;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task Like(int id)
        {
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            await _questionServices.Like(id, user);
        }

        [HttpPost]
        public async Task Dislike(int id)
        {
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            await _questionServices.Dislike(id, user);
        }

        [HttpGet]
        public IActionResult Questions()
        {
            var user = _db.Users.Include(u => u.Likes).FirstOrDefault(u => u.UserName == User.Identity.Name);
            ViewBag.User = user;

            return View(_db.Questions
                .Include(q => q.LikesList)
                .ToList());
        }

        [HttpGet]
        public IActionResult Question(int id)
        {
            var replies = _questionServices.GetReplies(id);
            if (replies != null)
            {
                ViewData["Replies"] = replies;
                return View(_db.Questions.First(q => q.Id == id));
            }
            else
            {
                return RedirectToAction("Questions");
            }
        }

        [HttpGet]
        public IActionResult Recent()
        {
            return View("Questions", _db.Questions.OrderBy(q => q.Date).ToList());
        }

        [HttpGet]
        public IActionResult Popular()
        {
            return View("Questions", _db.Questions.OrderBy(q => q.Likes).ToList());
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            return View(_db.Topics.ToList());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(AddQuestionViewModel model)
        {
            await _questionServices.Add(model.Text, model.Topic, _userManager.GetUserAsync(HttpContext.User).Result);
            return RedirectToAction("Questions");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var question = _db.Questions.First(q => q.Id == id);
            if (_userManager.GetUserId(HttpContext.User) == question.UserId)
            {
                return View(question);
            }
            else
            {
                return RedirectToAction("Questions");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(EditQuestionViewModel model)
        {
            var question = _db.Questions.First(q => q.Id == model.Qid);
            if (_userManager.GetUserId(HttpContext.User) == question.UserId)
            {
                _questionServices.Edit(model.Text, question, model.Qid);
                return RedirectToAction("Questions");
            }
            else
            {
                return RedirectToAction("Questions");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (_userManager.GetUserId(HttpContext.User) == _db.Questions.First(q => q.Id == id).UserId)
            {
                await _questionServices.Delete(id);
                return RedirectToAction("Questions");
            }
            else
            {
                return RedirectToAction("Questions");
            }
        }
        [HttpGet]
        public IActionResult SelectedQuestions(string id)
        {
            var user = _db.Users.Include(u => u.Likes).FirstOrDefault(u => u.UserName == User.Identity.Name);
            ViewBag.User = user;
      
            return View("Questions", _db.Questions.Where(q => q.TopicTitle == id).Include(q => q.LikesList).ToList());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reply(ReplyViewModel model, int qid)
        {
            await _questionServices.Reply(model.Reply, qid, _userManager.GetUserAsync(HttpContext.User).Result);
            return RedirectToAction("Question", new { id = qid });
        }
        [HttpGet]
        public IActionResult ShowTopics()
        {
            return View(_db.Topics.ToList());
        } 
        
    }
}
