using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab1.Models;
using Microsoft.AspNet.Identity;

namespace Lab1.Controllers
{
    public class BlogsController : Controller
    {
        const int CAFP = 10; // COUNT_ARTICLES_FOR_PAGE - количество статей на странице
        ApplicationDbContext db = new ApplicationDbContext();

        public enum MessageStatus
        {
            ArticleDeleted,//Статья удалена
            DeletedArticleNotFound,//Удаляемая статья не существует
            ArticleCreated,//Статья добавлена
            EditedArticleNotFound,//Редактируемая статья не найдена
            ArticleEdited,//Статья отредактирована
            ArticleAlreadyFavorites,//Статья уже в избранном
            ArticleAddFaavorites,//Статья добавлена в избранное
            AddFavoritesArticleOrUserNotFound,//Не найден пользователь или статья для добавления ее в Избранное
            DeleteFavoritesArticleAndUserNotFound,//Удаляемая из избранного статья и пользователь не найдены
            UserForFavoritesNotFound//Не найден пользователя для открытия его избранных статей
        }

        [HttpGet]
        public ActionResult Index(int? CategoryID, int? page, MessageStatus message)
        {
            //Определитель сообщения
            ViewBag.Message = 
                message == MessageStatus.ArticleDeleted ? "Статья удалена!"
                : message == MessageStatus.DeletedArticleNotFound ? "Статья, которую вы пытаетесь удалить, не существует."
                : message == MessageStatus.ArticleCreated ? "Статья успешно добавлена!"
                : message == MessageStatus.EditedArticleNotFound ? "Статья, которую вы пытались отредактировать, не существует."
                : message == MessageStatus.ArticleEdited ? "Изменения успешно зафиксированы!"
                : message == MessageStatus.ArticleAlreadyFavorites ? "Статья уже находится у вас в Избранном."
                : message == MessageStatus.ArticleAddFaavorites ? "Статья успешно добавлена в избранное!"
                : message == MessageStatus.AddFavoritesArticleOrUserNotFound ? "Не найден пользователь или статья для добавления ее в Избранное."
                : message == MessageStatus.DeleteFavoritesArticleAndUserNotFound ? "Не найден пользователь и статья для удаления ее из Избранного."
                : message == MessageStatus.UserForFavoritesNotFound ? "Не определен текущий пользователь, чтобы открыть его Избранные статьи."
                : "";
            //Определитель цвета оповещения
            ViewBag.MessageCode = 
                message == MessageStatus.DeletedArticleNotFound ? 1
                : message == MessageStatus.EditedArticleNotFound ? 1
                : message == MessageStatus.ArticleAlreadyFavorites ? 1
                : message == MessageStatus.AddFavoritesArticleOrUserNotFound ? 1
                : message == MessageStatus.DeleteFavoritesArticleAndUserNotFound ? 1
                : message == MessageStatus.UserForFavoritesNotFound ? 1
                : 0;

            ViewBag.Categorys = db.Categorys;
            ViewBag.Favorites = db.Favorites;
            ViewBag.categoryID = 0;
            if (CategoryID != null)
            {
                ViewBag.categoryID = CategoryID;
                ViewBag.Authors = db.Users;
                List<Article> ArticleForLooking = db.Articles
                    .Where(x => x.CategoryID == CategoryID)
                    .ToList();
                ArticleForLooking.Reverse();

                ViewBag.CountPage = (int)Math.Ceiling((double)ArticleForLooking.Count / CAFP);
                ViewBag.FirstPageDiasbled = "";
                ViewBag.LastPageDiasbled = "";
                // Если статей меньше CAFP в этой категории, то выводим все
                if (page == null)
                {
                    // Если больше CAFP, но пользователь не указывал какую страницу открыть, то
                    // открываем превую
                    if (ViewBag.CountPage > 1)
                    {
                        ArticleForLooking = ArticleForLooking.GetRange(0, CAFP);
                        ViewBag.CurrentPage = 1; //В этом случае текущей страницей будет первая
                        ViewBag.FirstPageDiasbled = "disabled"; //Выключаем возврат на предыдующую страницу
                    }
                }
                else
                {
                    // Если статей больше CAFP в этой категории, то выводим CAFP статей для указанной страницы

                    //Если на последней странице нет CAFP статей
                    if (CAFP * (int)page >= ArticleForLooking.Count)
                    {
                        ArticleForLooking = ArticleForLooking.GetRange(CAFP * ((int)page - 1), ArticleForLooking.Count - CAFP * ((int)page - 1));
                        ViewBag.LastPageDiasbled = "disabled"; //Выключаем переход на следующую страницу
                    }
                    else
                    {
                        //Если на странице есть CAFP статей, то берем их
                        ArticleForLooking = ArticleForLooking.GetRange(CAFP * ((int)page - 1), CAFP);
                    }
                    ViewBag.CurrentPage = page; //Получаем тукущую страницу
                    if (page == 1) ViewBag.FirstPageDiasbled = "disabled";
                }
                return View(ArticleForLooking);
            }

            return View();
        }

        [Authorize]
        public ActionResult Favorites(int? page, int? message)
        {
            string CurrentUserID = User.Identity.GetUserId();
            if (CurrentUserID == null) return RedirectToAction("Index", "Blogs", new { message = 10 });
            switch (message)
            {
                case 1:
                    ViewBag.Message = "Статья успешно удалена из вашего избранного!";
                    break;
                case 2:
                    ViewBag.Message = "У вас в Избранном нет статьи, которую вы пытаетесь удалить.";
                    ViewBag.MessageCode = 1;
                    break;
                case 3:
                    ViewBag.Message = "Не найдена статья для удаления ее из Избранного.";
                    ViewBag.MessageCode = 1;
                    break;
                case 4:
                    ViewBag.Message = "Статья удалена!";
                    break;
                case 5:
                    ViewBag.Message = "Статья, которую вы пытаетесь удалить, не существует.";
                    ViewBag.MessageCode = 1;
                    break;
            }

            ViewBag.Categorys = db.Categorys;
            ViewBag.Articles = db.Articles;
            ViewBag.Authors = db.Users;

            List<Favorites> favorites = db.Favorites.Where(x => x.UserID == CurrentUserID).ToList();
            favorites.Reverse();

            ViewBag.CountPage = (int)Math.Ceiling((double)favorites.Count / CAFP);
            ViewBag.FirstPageDiasbled = "";
            ViewBag.LastPageDiasbled = "";
            // Если статей меньше CAFP в Избранном, то выводим все
            if (page == null)
            {
                // Если больше CAFP, но пользователь не указывал какую страницу открыть, то
                // открываем превую
                if (ViewBag.CountPage > 1)
                {
                    favorites = favorites.GetRange(0, CAFP);
                    ViewBag.CurrentPage = 1; //В этом случае текущей страницей будет первая
                    ViewBag.FirstPageDiasbled = "disabled"; //Выключаем возврат на предыдующую страницу
                }
            }
            else
            {
                // Если статей больше CAFP в Избранном, то выводим CAFP статей для указанной страницы

                //Если на последней странице нет CAFP статей
                if (CAFP * (int)page >= favorites.Count)
                {
                    favorites = favorites.GetRange(CAFP * ((int)page - 1), favorites.Count() - CAFP * ((int)page - 1));
                    ViewBag.LastPageDiasbled = "disabled"; //Выключаем переход на следующую страницу
                }
                else
                {
                    //Если на странице есть CAFP статей, то берем их
                    favorites = favorites.GetRange(CAFP * ((int)page - 1), CAFP);
                }
                ViewBag.CurrentPage = page; //Получаем тукущую страницу
                if (page == 1) ViewBag.FirstPageDiasbled = "disabled";
            }
            return View(favorites);
        }

        //Метод добавления статьи в избранное
        [Authorize]
        public ActionResult AddFavorites(int? ArticleID, int? page)
        {
            int message = 8;
            string CurrentUserID = User.Identity.GetUserId();
            if (db.Users.Find(CurrentUserID) != null && db.Articles.Find(ArticleID) != null)
            {
                message = 6;
                if (db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList().Count() == 0)
                {
                    db.Favorites.Add(new Favorites { UserID = CurrentUserID, ArticleID = (int)ArticleID });
                    db.SaveChanges();
                    message = 7;
                }
                return new RedirectResult(Url.Action("Index", "Blogs", new { db.Articles.Find(ArticleID).CategoryID, page, message }) + "#article" + ArticleID);
            }
            else
            {
                return RedirectToAction("Index", "Blogs", new { message });
            }
        }

        //Метод удаления статьи из избранного
        [Authorize]
        public ActionResult DelFavorites(int? ArticleID, int? page, int? NextArticleID)
        {
            string CurrentUserID = User.Identity.GetUserId();
            int message = 0;
            if (db.Users.Find(CurrentUserID) != null && db.Articles.Find(ArticleID) != null)
            {
                message = 2;
                if (db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList().Count > 0)
                {
                    Favorites fav = db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList()[0];
                    db.Favorites.Remove(fav);
                    db.SaveChanges();
                    message = 1;
                }
                return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message }) + "#article" + NextArticleID);
            }
            else
            {
                message = 3;
                if (db.Users.Find(CurrentUserID) != null)
                {
                    return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message }) + "#article" + NextArticleID);
                }
                else
                {
                    message = 9;
                    return new RedirectResult(Url.Action("Index", "Blogs", new { page, message }) + "#article" + NextArticleID);
                }
            }
        }

        //Метод удаления статьи
        [Authorize]
        public ActionResult Delete(int? CategoryID, int? ArticleID, int? page, int? NextArticleID)
        {
            string CurrentUserID = User.Identity.GetUserId();
            Article art = db.Articles.Find(ArticleID);
            if (art != null)
            {
                List<Favorites> favorites = db.Favorites.Where(x => x.ArticleID == ArticleID).ToList();
                if (favorites.Count > 0)
                {
                    foreach (Favorites fav in favorites)
                    {
                        db.Favorites.Remove(fav);
                    }
                }
                int catID = art.CategoryID;
                db.Articles.Remove(art);
                db.SaveChanges();
                if (CurrentUserID == null)
                {
                    return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID = catID, page, message = 1 }) + "#article" + NextArticleID);
                }
                else
                {
                    return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message = 4 }) + "#article" + NextArticleID);
                }
            }
            if (CurrentUserID == null)
            {
                return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID, page, message = 2 }) + "#article" + NextArticleID);
            }
            else
            {
                return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message = 5 }) + "#article" + NextArticleID);
            }
        }

        //Метод отображения страницы создания статьи
        [Authorize]
        public ActionResult Create(int? CategoryID, string Subject, int? message)
        {
            if (message == 1) ViewBag.Message = "Все поля обязательны для заполнения";
            ViewBag.Subject = Subject;
            List<Category> CategoryForLooking = db.Categorys.ToList();
            return View(CategoryForLooking);
        }

        //Метод создания статьи
        [HttpPost]
        [Authorize]
        public ActionResult Create(Article art)
        {
            if (art.Subject != null && art.Subject != "" && art.Text != null && art.Text != "" && art.CategoryID != 0)
            {
                db.Articles.Add(art);
                db.SaveChanges();
                return RedirectToAction("Index", "Blogs", new { art.CategoryID, message = 3 });
            }
            return RedirectToAction("Create", "Blogs", new { art.CategoryID, art.Subject, message = 1 });
        }

        //Метод отображения страницы редактирования статьи
        [Authorize]
        public ActionResult Edit(int? ArticleID, string Subject, int? message, int? page, int? CategoryID)
        {
            if (message == 1) ViewBag.Message = "Все поля обязательны для заполнения";
            ViewBag.Subject = Subject;
            Article art = db.Articles.Find(ArticleID);
            ViewBag.Categorys = db.Categorys.ToList();
            if (art == null) return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID, page, message = 4 }) + "#article" + ArticleID);
            return View(art);
        }

        //Метод редактирования статьи
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Article art)
        {
            if (art.Subject != null && art.Subject != "" && art.Text != null && art.Text != "" && art.CategoryID != 0)
            {
                db.Articles.AddOrUpdate(art);
                db.SaveChanges();

                //считаем какая будет новая страница для отредактированной статьи
                List<Article> articles = db.Articles.Where(x => x.CategoryID == art.CategoryID).ToList();
                articles.Reverse();
                int number = 0;
                for (int i=0; i<articles.Count(); i++)
                {
                    if(articles[i].ID == art.ID)
                    {
                        number = i + 1;
                        break;
                    }
                }
                int page = (int)Math.Ceiling((double)number / CAFP);

                return new RedirectResult(Url.Action("Index", "Blogs", new { art.CategoryID, page, message = 5 }) + "#article" + art.ID);
            }
            return RedirectToAction("Edit", "Blogs", new { ArticleID = art.ID, art.Subject, message = 1 });
        }
    }
}