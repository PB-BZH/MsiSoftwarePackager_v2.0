<?php
declare(strict_types=1);

$pageTitle = 'Politique de confidentialité - PB BZH Concept';
$lastUpdate = '11 juin 2026';
?>
<!DOCTYPE html>
<html lang="fr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title><?= htmlspecialchars($pageTitle, ENT_QUOTES, 'UTF-8') ?></title>
    <meta name="description" content="Politique de confidentialité du site de téléchargement de logiciels PB BZH Concept.">
    <meta name="robots" content="index,follow">
    
    <link rel="icon" href="/softwares/assets/pb-bzh-logo.png" type="image/png">
    <link rel="shortcut icon" href="/softwares/assets/pb-bzh-logo.png" type="image/png">
    <link rel="apple-touch-icon" href="/softwares/assets/pb-bzh-logo.png">

  <style>
    :root {
      --bg: #f5f7fb;
      --card: #ffffff;
      --text: #1f2937;
      --muted: #6b7280;
      --border: #d1d5db;
      --accent: #2563eb;
      --accent-dark: #1e40af;
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      padding: 0;
      font-family: Arial, Helvetica, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }

    header {
        background: linear-gradient(135deg, #111827, #1f2937);
        color: white;
        padding: 32px 20px;
    }

    .header-inner {
        max-width: 980px;
        margin: 0 auto;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 22px;
    }

    header img {
        width: 124px;
        height: 124px;
        object-fit: contain;
        border-radius: 12px;
        flex: 0 0 auto;
    }

    .header-text {
        display: flex;
        flex-direction: column;
        justify-content: center;
    }

    header h1 {
        margin: 0 0 8px 0;
        font-size: 2rem;
        line-height: 1.2;
    }

    header p {
        margin: 0;
        color: #d1d5db;
    }

    header a,
    footer a {
      color: #bfdbfe;
      text-decoration: none;
    }

    header a:hover,
    footer a:hover {
      color: white;
      text-decoration: underline;
    }

    @media (max-width: 640px) {
        .header-inner {
            flex-direction: column;
            text-align: center;
        }

        header img {
            width: 110px;
            height: 110px;
        }

        header h1 {
            font-size: 1.6rem;
        }
    }

    main {
      max-width: 980px;
      margin: 32px auto;
      padding: 0 20px;
    }

    .card {
      background: var(--card);
      border: 1px solid var(--border);
      border-radius: 12px;
      padding: 28px;
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
    }

    h2 {
      margin-top: 32px;
      color: #111827;
      border-bottom: 1px solid var(--border);
      padding-bottom: 8px;
    }

    h2:first-child {
      margin-top: 0;
    }

    a {
      color: var(--accent);
      text-decoration: none;
    }

    a:hover {
      color: var(--accent-dark);
      text-decoration: underline;
    }

    ul {
      padding-left: 22px;
    }

    .notice {
      background: #eff6ff;
      border-left: 4px solid var(--accent);
      padding: 14px 16px;
      margin: 20px 0;
      border-radius: 8px;
    }

    .muted {
      color: var(--muted);
      font-size: 0.95rem;
    }

    footer {
      text-align: center;
      color: #d1d5db;
      background: linear-gradient(135deg, #111827, #1f2937);
      padding: 24px 20px 40px;
      font-size: 0.95rem;
    }

    footer p {
      margin: 6px 0;
    }
  </style>
</head>
<body>

<header>
  <div class="header-inner">
    <img src="/softwares/assets/pb-bzh-logo.png" alt="PB-BZH Concept">

    <div class="header-text">
      <h1>Politique de confidentialité</h1>
      <p>PB BZH Concept - Site de téléchargement de logiciels</p>
      <p>contact : <a href="mailto:admin@pb-bzh-concept.fr">admin@pb-bzh-concept.fr</a></p>

    </div>
  </div>
</header>
<main>
  <section class="card">
    <p class="muted">
      Dernière mise à jour : <?= htmlspecialchars($lastUpdate, ENT_QUOTES, 'UTF-8') ?>
    </p>

    <div class="notice">
      Cette page explique de manière simple quelles données peuvent être traitées lors de l’utilisation
      du site <strong>softwares.pb-bzh-concept.fr</strong> et des logiciels proposés au téléchargement.
    </div>

    <h2>1. Éditeur du site</h2>

    <p>
      Le site <strong>softwares.pb-bzh-concept.fr</strong> est édité par <strong>PB BZH Concept</strong>.
    </p>

    <p>
      Ce site a pour objectif de présenter et de distribuer des logiciels développés ou publiés par
      PB BZH Concept, notamment sous forme d’installateurs MSI, de programmes d’installation web
      et de fichiers de mise à jour.
    </p>

    <h2>2. Données collectées directement</h2>

    <p>
      Le site ne propose pas de création de compte utilisateur et ne demande pas volontairement
      d’informations personnelles pour télécharger les logiciels.
    </p>

    <p>
      Aucun formulaire de contact, d’inscription ou de paiement n’est actuellement utilisé sur ce site.
    </p>

    <h2>3. Données techniques pouvant être traitées</h2>

    <p>
      Lors de la consultation du site ou du téléchargement d’un fichier, certaines données techniques
      peuvent être enregistrées automatiquement par l’hébergeur ou le serveur web, notamment :
    </p>

    <ul>
      <li>l’adresse IP utilisée pour accéder au site ;</li>
      <li>la date et l’heure de la requête ;</li>
      <li>l’adresse de la page ou du fichier demandé ;</li>
      <li>le navigateur ou le système d’exploitation déclaré par le terminal ;</li>
      <li>les erreurs techniques éventuelles.</li>
    </ul>

    <p>
      Ces informations sont utilisées uniquement pour assurer le bon fonctionnement du site,
      la sécurité du service, le diagnostic technique, la prévention des abus et la résolution
      d’éventuels incidents.
    </p>

    <h2>4. Téléchargements et mises à jour logicielles</h2>

    <p>
      Les logiciels proposés peuvent accéder à des fichiers publics de mise à jour, par exemple :
    </p>

    <ul>
      <li>des manifestes de version ;</li>
      <li>des fichiers <code>update.json</code> ;</li>
      <li>des installateurs MSI ou exécutables de mise à jour.</li>
    </ul>

    <p>
      Ces accès servent uniquement à vérifier la disponibilité d’une nouvelle version ou à télécharger
      un installateur. Aucune donnée personnelle n’est volontairement transmise par ces mécanismes,
      en dehors des informations techniques nécessaires à toute connexion HTTP/HTTPS.
    </p>

    <h2>5. Cookies et traceurs</h2>

    <p>
      Le site n’utilise pas volontairement de cookies publicitaires, de cookies de suivi marketing,
      ni de système de profilage utilisateur.
    </p>

    <p>
      Si des cookies strictement nécessaires au fonctionnement technique du site devaient être utilisés,
      ils auraient uniquement pour finalité d’assurer le bon fonctionnement du service.
    </p>

    <h2>6. Partage des données</h2>

    <p>
      Les données techniques éventuellement enregistrées ne sont pas vendues et ne sont pas utilisées
      à des fins publicitaires.
    </p>

    <p>
      Elles peuvent être traitées par les prestataires techniques nécessaires au fonctionnement du site,
      notamment l’hébergeur, dans la limite de leurs missions techniques.
    </p>

    <h2>7. Durée de conservation</h2>

    <p>
      Les données techniques de connexion, lorsqu’elles existent, sont conservées pour une durée limitée
      nécessaire à la sécurité, au diagnostic et à l’administration du site.
    </p>

    <p>
      Les fichiers logiciels publiés, les manifestes de mise à jour et les pages du catalogue peuvent être
      conservés aussi longtemps qu’ils sont nécessaires à la distribution ou à la maintenance des logiciels.
    </p>

    <h2>8. Sécurité</h2>

    <p>
      Les logiciels publiés peuvent être signés numériquement afin de permettre aux utilisateurs de vérifier
      l’identité de l’éditeur et l’intégrité des fichiers téléchargés.
    </p>

    <p>
      Le site utilise une connexion HTTPS afin de protéger les échanges entre le navigateur de l’utilisateur
      et le serveur.
    </p>

    <h2>9. Droits des utilisateurs</h2>

    <p>
      Conformément à la réglementation applicable en matière de protection des données, les utilisateurs
      peuvent demander l’accès, la rectification ou la suppression des données personnelles les concernant,
      lorsque de telles données existent.
    </p>

    <p>
      Pour toute demande relative à la confidentialité ou aux données personnelles, vous pouvez contacter :
    </p>

    <p>
      <strong>PB BZH Concept</strong><br>
      contact : <a href="mailto:admin@pb-bzh-concept.fr">admin@pb-bzh-concept.fr</a>
    </p>

    <h2>10. Modification de cette politique</h2>

    <p>
      Cette politique de confidentialité peut être mise à jour afin de tenir compte de l’évolution du site,
      des logiciels distribués ou de la réglementation applicable.
    </p>

    <p>
      La date de dernière mise à jour est indiquée en haut de cette page.
    </p>

    <h2>11. Retour au site</h2>

    <p>
      <a href="/softwares/">Retour au catalogue des logiciels</a>
    </p>
  </section>
</main>

<footer>
  <p>© <?= date('Y') ?> PB BZH Concept - Tous droits réservés.</p>
  <p>contact : <a href="mailto:admin@pb-bzh-concept.fr">admin@pb-bzh-concept.fr</a></p>
  <p><a href="/softwares/">Retour au catalogue des logiciels</a></p>
</footer>

</body>
</html>
