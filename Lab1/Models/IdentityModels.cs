using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
namespace Lab1.Models
{
    // Чтобы добавить данные профиля для пользователя, можно добавить дополнительные свойства в класс ApplicationUser. Дополнительные сведения см. по адресу: http://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDay { get; set; }
        public string Sex { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Favorites> Favorites { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class AppDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext db)
        {
            db.Categorys.Add(new Category { Name = "Новости" });
            db.Categorys.Add(new Category { Name = "IT" });
            db.Categorys.Add(new Category { Name = "HiTech" });
            db.Categorys.Add(new Category { Name = "Кулинария" });
            db.Categorys.Add(new Category { Name = "Кино" });
            db.Categorys.Add(new Category { Name = "Музыка" });
            db.Categorys.Add(new Category { Name = "Красота и здоровье" });
            db.Categorys.Add(new Category { Name = "Транспорт" });
            db.Categorys.Add(new Category { Name = "Финансы и бизнес" });

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            roleManager.Create(new IdentityRole("user"));
            roleManager.Create(new IdentityRole("admin"));

            ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            var user = new ApplicationUser
            {
                LastName = "Ковальчук",
                FirstName = "Антон",
                MiddleName = "Дмитриевич",
                UserName = "antony.nor@gmail.com",
                BirthDay = new DateTime(1996, 9, 28),
                Email = "antony.nor@gmail.com",
                Sex = "Мужской",
                PhoneNumber = "+79158562772",
                LockoutEnabled = false
            };
            var result = manager.Create(user, "123kT@");
            if(result.Succeeded)
            {
                manager.AddToRole(user.Id, "admin");
            }

            db.Articles.Add(new Article
            {
                CategoryID = 1,
                AuthorID = user.Id,
                Subject = "Захарова пообещала \"сюрприз\" для Лондона на ближайшем брифинге",
                Text = "БЛАГОВЕЩЕНСК, 25 мар — РИА Новости. Официальный представитель МИД России " +
                "Мария Захарова считает, что Великобритания в \"деле Скрипаля\" применяет приемы черного пиара, а сами действия Лондона являются \"колоссальной провокацией и авантюрой\". Об этом она заявила в эфире телеканала \"Россия 1\".\r\n" +
                "" +
                "Они не могут отстоять свои тезисы, <…> они не могут это продать, используя свои собственные доводы. Соответственно, перешли к концепции антирекламы и черного пиара мирового масштаба.Это — колоссальная провокация. Я бы сказала больше: это — колоссальная авантюра\", — сказала она в программе " +
                "\"Воскресный вечер с Владимиром Соловьевым\".\r\n" +
                "" +
                "Захарова отметила, что информационная кампания против России плохо проработана. Так, по ее словам, на неубедительную аргументацию уже начали обращать внимание британские СМИ.\r\n" +
                "" +
                "Официальный представитель МИД также подчеркнула, что саммит ЕС обсудил инцидент в Солсбери \"без единого факта, без единого аргумента\", что, как она считает, \"даже не похоже на судилище\".\r\n" +
                "" +
                "\"Дай хоть какой-то аргумент, нет, нужно сравнение с Третьим рейхом и нацистской Германией в таких публицистических целях\", — сказала она.\r\n" +
                "" +
                "Дипломат также пообещала сюрприз всем тем, кто пытается представить Россию в образе врага. \"У нас есть сюрприз для всех, у кого хватает либо совести, либо наглости сравнить, хоть как - то проводить параллели между " +
                "Россией и Третьим рейхом\", — заявила Захарова.\r\n" +
                "" +
                "В британском Солсбери 4 марта отравили экс - офицера ГРУ Сергея Скрипаля,работавшего на британские спецслужбы, и его дочь Юлию. Власти Соединенного Королевства обвинили в этом Россию, не дождавшись результатов " +
                "расследования и не предъявив никаких доказательств.\r\n" +
                "" +
                "Директор департамента по вопросам нераспространения и контроля над вооружениями МИД России Владимир Ермаков в среду заявил, что Москва считает нападение на российских  граждан террористическим актом.При этом он предположил," +
                "что Лондон мог срежиссировать инцидент.\r\n" +
                "" +
                "Глава МИД России Сергей Лавров, в свою очередь, подчеркивал, что Москва готова к совместному расследованию дела в том случае, если Лондон будет соблюдать все формальные процедуры, " +
                "предусмотренные Конвенцией о запрете химического оружия.",
                DT = new DateTime(2018, 4, 3, 18, 1, 0)
            });

            db.Articles.Add(new Article
            {
                CategoryID = 1,
                AuthorID = user.Id,
                Subject = "Йемен запустил ракеты на Саудовскую Аравию",
                Text = "Йеменские боевики Ансаруллы и подразделения союзных армий провели ракетные атаки против Саудовской Аравии, нацелившись на несколько позиций в королевстве, сообщает kratko-news.com.\r\n" +
                "" +
                "Военные силы Йемена нацелились на международный аэропорт Кинга Халида в Эр-Рияде, а также на Абха, Наджран и Джизанские региональные аэропорты с баллистическими ракетами, но военно - воздушные " +
                "силы Саудовской Аравии перехватили ракеты над северо - восточной частью столицы Эр-Рияд поздно вечером в воскресенье, сообщает государственное телевидение Саудовской Аравии.\r\n" +
                "" +
                "Ракетные атаки приходят в третью годовщину нападения Саудовской Аравии на своего южного соседа.\r\n" +
                "" +
                "По мнению международных учреждений, продолжение воздушной, морской и земельной блокады Йемена со стороны Саудовской Аравии привело к худшей гуманитарной ситуации в мире, в результате чего более 20 миллионов " +
                "йеменцев оказались на пороге голода и холода.\r\n" +
                "" +
                "Более 14 000 человек были убиты с начала военной кампании Саудовской Аравии против Йемена в марте 2015 года. Большая часть инфраструктуры арабского полуострова, включая " +
                "больницы, школы и фабрики, была сведена к руинам из - за войны.",
                DT = new DateTime(2018, 4, 3, 18, 1, 0)
            });

            for (int i = 0; i < 30; i++)
            {
                db.Articles.Add(new Article
                {
                    CategoryID = 2,
                    AuthorID = user.Id,
                    Subject = "Тема" + (i + 1),
                    Text = "Текст" + (i + 1),
                    DT = DateTime.Now
                });
            }

            for (int i = 30; i < 57; i++)
            {
                db.Favorites.Add(new Favorites { UserID = user.Id, ArticleID = i });
            }

            user = new ApplicationUser
            {
                LastName ="Грудинин",
                FirstName = "Павел",
                MiddleName = "Викторович",
                UserName = "grudinin@gmail.com",
                BirthDay = new DateTime(1978, 11, 25),
                Email = "grudinin@gmail.com",
                Sex = "Мужской",
                PhoneNumber = "+79665554444",
                LockoutEnabled = true
            };
            result = manager.Create(user, "321kT@");
            if (result.Succeeded)
            {
                manager.AddToRole(user.Id, "user");
            }

            db.Articles.Add(new Article
            {
                CategoryID = 1,
                AuthorID = user.Id,
                Subject = "Пассажирский автобус столкнулся с иномаркой: есть пострадавший",
                Text = "В ликвидации последствий аварии приняли участие спасатели. Сегодня, 2 апреля, в 9:10 поступило сообщение о ДТП, произошедшем на 12 километре автодороги Елец - Чернышевка в Елецкого района.Произошло " +
                "столкновение пассажирского автобуса «КАВЗ» и легкового автомобиля «Киа».\r\nВ результате произошедшего ДТП есть пострадавший, доставлены в ГУЗ «Елецкая ГБ № 1 им.Н.А.Семашко», сообщает пресс - служба ГУ МЧС России " +
                "по Липецкой области.",
                DT = new DateTime(2018, 3, 28, 18, 12, 0)
            });

            for (int i = 30; i < 60; i++)
            {
                db.Articles.Add(new Article
                {
                    CategoryID = 2,
                    AuthorID = user.Id,
                    Subject = "Тема" + (i + 1),
                    Text = "Текст" + (i + 1),
                    DT = DateTime.Now
                });
            }

            base.Seed(db);
        }
    }
}