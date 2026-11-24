# 🎁 Wishly

**Wishly** est une plateforme web de listes de souhaits (wishlists). Chaque personne crée ses listes de cadeaux, les partage avec ses proches grâce à un lien public, et ses amis peuvent **réserver un cadeau** sans que le créateur de la liste ne sache lequel a été choisi. Fini les cadeaux en double !

Projet universitaire réalisé avec **.NET 8**, **React** et **MySQL**.

---

## ✨ Fonctionnalités

- **Compte utilisateur** : inscription et connexion classiques, ou avec Google
- **Listes de souhaits** : nom, occasion, thème visuel, date de l'événement
- **Cadeaux** : ajout par lien (titre, photo et prix récupérés automatiquement), par URL d'image ou par envoi de fichier
- **Partage** : chaque liste possède un lien public unique
- **Réservation** : un invité réserve un cadeau, et un seul par liste
- **Surprise préservée** : le créateur voit uniquement « réservé » ou « disponible », jamais qui a réservé
- **Protection contre les réservations simultanées** : si deux personnes réservent le même cadeau au même moment, une seule réussit
- **Espace « Mon compte »** : mes listes, cadeaux que j'ai réservés, organisation d'événements
- **Notifications** pour suivre les réservations
- **Profil modifiable** : nom, e-mail, photo, mot de passe
- **Page Shop** : accès rapide à des boutiques (mode, beauté, bijoux, déco) pour trouver des idées
- **Confirmation par e-mail** (« Oui, c'est moi ») avant d'ouvrir une liste partagée, et e-mail de remerciement après une réservation
- **Site responsive** : adapté aux ordinateurs, tablettes et téléphones

---

## 🧱 Technologies

| Partie | Technologies |
| --- | --- |
| Backend | ASP.NET Core Web API (.NET 8), Entity Framework Core 8, Pomelo MySQL |
| Base de données | MySQL 8 |
| Frontend | React (Vite), React Router, Axios, `@react-oauth/google` |
| Authentification | JWT (Bearer), BCrypt, Google Sign-In |
| E-mails | MailKit (SMTP Gmail) |
| Extraction de liens | HtmlAgilityPack |

---

## 🏗️ Architecture

Le backend suit une **architecture en couches** (Clean Architecture) :

```
Wishly/
├── Wishly.Domain/          # Entités (User, Wishlist, Item, Reservation...)
├── Wishly.Application/     # DTOs et contrats
├── Wishly.Infrastructure/  # EF Core, DbContext, services (UserService, EmailService...)
└── Wishly.Api/             # Contrôleurs, Program.cs, configuration

wishly-frontend/
├── src/
│   ├── pages/              # Home, Login, Register, MonCompte, PublicWishlist, Shop...
│   ├── components/         # Toast, Notifications, GoogleGate...
│   └── services/api.js     # Configuration Axios
```

### Modèle de données

`Users` → `Wishlists` → `Items` → `Reservations`

La table `Reservations` possède un index **UNIQUE sur `ItemId`** : c'est la base de données elle-même qui garantit qu'un cadeau ne peut être réservé qu'une seule fois, même en cas de requêtes simultanées. L'API intercepte l'erreur et répond avec un code `409`.

---

## 🚀 Installation en local

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18 ou plus
- [MySQL 8](https://dev.mysql.com/downloads/)

### 1. Base de données

Crée une base nommée `wishly` dans MySQL, puis exécute le script SQL fourni (`wishly_database.sql`) pour créer les tables.

### 2. Backend

Crée le fichier `Wishly.Api/appsettings.Development.json` :

```json
{
  "ConnectionStrings": {
    "WishlyDb": "Server=localhost;Port=3306;Database=wishly;User=root;Password='VOTRE_MOT_DE_PASSE';"
  },
  "Jwt": {
    "Key": "UNE_CLE_SECRETE_D_AU_MOINS_32_CARACTERES"
  },
  "FrontendUrl": "http://localhost:5173",
  "Google": {
    "ClientId": "VOTRE_CLIENT_ID_GOOGLE.apps.googleusercontent.com"
  },
  "EmailSettings": {
    "SenderName": "Wishly",
    "SenderEmail": "votre.adresse@gmail.com",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "AppPassword": "MOT_DE_PASSE_D_APPLICATION_GMAIL"
  }
}
```

Puis lance l'API :

```bash
cd Wishly.Api
dotnet run
```

L'API écoute sur `https://localhost:7004`.

### 3. Frontend

```bash
cd wishly-frontend
npm install
npm run dev
```

Le site est accessible sur `http://localhost:5173`.

### Générer une clé JWT

Dans PowerShell :

```powershell
$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

### Configurer Google Sign-In

Dans la [Google Cloud Console](https://console.cloud.google.com), crée un identifiant OAuth de type « Application Web » et ajoute dans les **origines JavaScript autorisées** :

- `http://localhost:5173`
- l'adresse HTTPS de ton site une fois déployé

> ⚠️ Google n'accepte que des adresses `https` (ou `localhost`). Le bouton Google n'est donc affiché que dans ces cas.

---

## 🌐 Déploiement

1. Construire le front : `npm run build`
2. Copier le contenu de `dist/` dans `Wishly.Api/wwwroot/`
3. Créer `appsettings.Production.json` avec la chaîne de connexion de la base en ligne, la clé JWT, l'URL du site et les paramètres e-mail
4. Publier l'API (par exemple avec Web Deploy depuis Visual Studio)

L'API sert elle-même les fichiers du front (`UseStaticFiles` et `MapFallbackToFile`).

---

## 🔒 Sécurité

- Mots de passe hachés avec **BCrypt**
- Authentification par **jeton JWT**, routes sensibles protégées
- Vérification que seul le **propriétaire** peut modifier ou supprimer ses listes et cadeaux
- Seule la personne qui a réservé peut annuler sa réservation
- Limitation du nombre de requêtes par adresse IP sur les routes de connexion
- Liens de confirmation par e-mail à usage unique, valables 15 minutes
- Secrets (mots de passe, clés) **jamais versionnés** : ils sont placés dans les fichiers `appsettings.*.json`, ignorés par Git

