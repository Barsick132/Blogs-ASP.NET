using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab1.Models;
using Microsoft.AspNet.Identity;
using NLog;

namespace Lab1.Controllers
{
    public class BlogsController : Controller
    {
        const int CAFP = 10; // COUNT_ARTICLES_FOR_PAGE - количество статей на странице
        ApplicationDbContext db = new ApplicationDbContext();
        public Logger logger = LogManager.GetCurrentClassLogger();
        [HttpGet]
        public ActionResult Index(int? CategoryID, int? page, MessageStatus? message)
        {
            try
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
                //Определитель цвета оповещения: 1 - красный, 0 - синий
                switch (message)
                {
                    case MessageStatus.DeletedArticleNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Статья, которую вы пытаетесь удалить, не существует.");
                        break;
                    case MessageStatus.EditedArticleNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Статья, которую вы пытались отредактировать, не существует.");
                        break;
                    case MessageStatus.ArticleAlreadyFavorites:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Статья уже находится у вас в Избранном.");
                        break;
                    case MessageStatus.AddFavoritesArticleOrUserNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Не найден пользователь или статья для добавления ее в Избранное.");
                        break;
                    case MessageStatus.DeleteFavoritesArticleAndUserNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Не найден пользователь и статья для удаления ее из Избранного.");
                        break;
                    case MessageStatus.UserForFavoritesNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : Не определен текущий пользователь, чтобы открыть его Избранные статьи.");
                        break;
                    default:
                        ViewBag.MessageCode = 0;
                        break;

                }

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
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Index : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        [Authorize]
        public ActionResult Favorites(int? page, MessageStatus? message)
        {
            try
            {
                string CurrentUserID = User.Identity.GetUserId();
                if (CurrentUserID == null) return RedirectToAction("Index", "Blogs", new { MessageStatus.UserForFavoritesNotFound });

                //Определитель сообщения
                ViewBag.Message =
                    message == MessageStatus.ArticleDeletedFavorites ? "Статья успешно удалена из вашего избранного!"
                    : message == MessageStatus.DeletedArticleNotFoundFavorites ? "У вас в Избранном нет статьи, которую вы пытаетесь удалить."
                    : message == MessageStatus.ArticleDeletedFromFaavoritesNotFound ? "Не найдена статья для удаления ее из Избранного."
                    : message == MessageStatus.ArticleDeleted ? "Статья удалена!"
                    : message == MessageStatus.DeletedArticleNotFound ? "Статья, которую вы пытаетесь удалить, не существует."
                    : message == MessageStatus.EditedArticleNotFound ? "Статья, которую вы пытались отредактировать, не существует."
                    : "";
                //Определитель цвета оповещения: 1 - красный, 0 - синий
                switch(message)
                {
                    case MessageStatus.DeletedArticleNotFoundFavorites:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Favorites : У вас в Избранном нет статьи, которую вы пытаетесь удалить.");
                        break;
                    case MessageStatus.ArticleDeletedFromFaavoritesNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Favorites : Не найдена статья для удаления ее из Избранного.");
                        break;
                    case MessageStatus.DeletedArticleNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Favorites : Статья, которую вы пытаетесь удалить, не существует.");
                        break;
                    case MessageStatus.EditedArticleNotFound:
                        ViewBag.MessageCode = 1;
                        logger.Info(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Favorites : Статья, которую вы пытались отредактировать, не существует.");
                        break;
                    default:
                        ViewBag.MessageCode = 0;
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
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Favorites : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод добавления статьи в избранное
        [Authorize]
        public ActionResult AddFavorites(int? ArticleID, int? page)
        {
            try
            {
                MessageStatus message = MessageStatus.AddFavoritesArticleOrUserNotFound;
                string CurrentUserID = User.Identity.GetUserId();
                if (db.Users.Find(CurrentUserID) != null && db.Articles.Find(ArticleID) != null)
                {
                    message = MessageStatus.ArticleAlreadyFavorites;
                    if (db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList().Count() == 0)
                    {
                        db.Favorites.Add(new Favorites { UserID = CurrentUserID, ArticleID = (int)ArticleID });
                        db.SaveChanges();
                        message = MessageStatus.ArticleAddFaavorites;
                    }
                    return new RedirectResult(Url.Action("Index", "Blogs", new { db.Articles.Find(ArticleID).CategoryID, page, message }) + "#article" + ArticleID);
                }
                else
                {
                    return RedirectToAction("Index", "Blogs", new { message });
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/AddFavorites : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод удаления статьи из избранного
        [Authorize]
        public ActionResult DelFavorites(int? ArticleID, int? page, int? NextArticleID)
        {
            try
            {
                string CurrentUserID = User.Identity.GetUserId();
                MessageStatus message;
                if (db.Users.Find(CurrentUserID) != null && db.Articles.Find(ArticleID) != null)
                {
                    message = MessageStatus.DeletedArticleNotFoundFavorites;
                    if (db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList().Count > 0)
                    {
                        Favorites fav = db.Favorites.Where(x => x.UserID == CurrentUserID && x.ArticleID == ArticleID).ToList()[0];
                        db.Favorites.Remove(fav);
                        db.SaveChanges();
                        message = MessageStatus.ArticleDeletedFavorites;
                    }
                    return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message }) + "#article" + NextArticleID);
                }
                else
                {
                    message = MessageStatus.ArticleDeletedFromFaavoritesNotFound;
                    if (db.Users.Find(CurrentUserID) != null)
                    {
                        return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message }) + "#article" + NextArticleID);
                    }
                    else
                    {
                        message = MessageStatus.DeleteFavoritesArticleAndUserNotFound;
                        return new RedirectResult(Url.Action("Index", "Blogs", new { page, message }) + "#article" + NextArticleID);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/DelFavorites : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод удаления статьи
        [Authorize]
        public ActionResult Delete(bool? Favorites, int? CategoryID, int? ArticleID, int? page, int? NextArticleID)
        {
            try
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
                    if(Favorites!=null)
                    {
                        if((bool)Favorites)
                        {
                            return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message = MessageStatus.ArticleDeleted }) + "#article" + NextArticleID);
                        }
                    }
                    return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID = catID, page, message = MessageStatus.ArticleDeleted }) + "#article" + NextArticleID);
                }
                if (Favorites != null)
                {
                    if ((bool)Favorites)
                    {
                        return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message = MessageStatus.DeletedArticleNotFound }) + "#article" + NextArticleID);
                    }
                }
                return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID, page, message = MessageStatus.DeletedArticleNotFound }) + "#article" + NextArticleID);
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Delete : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод отображения страницы создания статьи
        [Authorize]
        public ActionResult Create(int? CategoryID)
        {
            try
            {
                ViewBag.CategoryForLooking = db.Categorys.ToList();
                return View();
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Create : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод создания статьи
        [HttpPost]
        [Authorize]
        public ActionResult Create(Article art)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Articles.Add(art);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Blogs", new { art.CategoryID, message = MessageStatus.ArticleCreated });
                }
                ViewBag.CategoryForLooking = db.Categorys.ToList();
                return View(art);
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Create POST : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }


        //Метод отображения страницы редактирования статьи
        [Authorize]
        public ActionResult Edit(int? ArticleID, int? page, int? CategoryID, bool? Favorites)
        {
            try
            {
                Article art = db.Articles.Find(ArticleID);
                if (art == null)
                {
                    if (Favorites != null)
                    {
                        if ((bool)Favorites)
                        {
                            return new RedirectResult(Url.Action("Favorites", "Blogs", new { page, message = MessageStatus.EditedArticleNotFound }) + "#article" + ArticleID);
                        }
                    }
                    return new RedirectResult(Url.Action("Index", "Blogs", new { CategoryID, page, message = MessageStatus.EditedArticleNotFound }) + "#article" + ArticleID);
                }
                ViewBag.Categorys = db.Categorys.ToList();
                return View(art);
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Edit : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

        //Метод редактирования статьи
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Article art)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Articles.AddOrUpdate(art);
                    db.SaveChanges();

                    //считаем какая будет новая страница для отредактированной статьи
                    List<Article> articles = db.Articles.Where(x => x.CategoryID == art.CategoryID).ToList();
                    articles.Reverse();
                    int number = 0;
                    for (int i = 0; i < articles.Count(); i++)
                    {
                        if (articles[i].ID == art.ID)
                        {
                            number = i + 1;
                            break;
                        }
                    }
                    int page = (int)Math.Ceiling((double)number / CAFP);

                    return new RedirectResult(Url.Action("Index", "Blogs", new { art.CategoryID, page, message = MessageStatus.ArticleEdited }) + "#article" + art.ID);
                }
                ViewBag.Categorys = db.Categorys.ToList();
                return View(art);
            }
            catch (Exception ex)
            {
                logger.Debug(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " in /Blogs/Edit POST : " + ex.Message);
                return RedirectToAction("Error", "Shared");
            }
        }

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
            UserForFavoritesNotFound,//Не найден пользователя для открытия его избранных статей

            ArticleDeletedFavorites,//Статья успешно удалена из избранного
            DeletedArticleNotFoundFavorites,//Удаляемая статья не найдена в избранном
            ArticleDeletedFromFaavoritesNotFound,//Не найдена статья для удаления из избранного

            AllFieldsRequired//Все поля обязательны для заполнения
        }
    }
}