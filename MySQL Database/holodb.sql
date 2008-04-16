/*
MySQL Data Transfer
Source Host: localhost
Source Database: holodb
Target Host: localhost
Target Database: holodb
Date: 16-4-2008 20:50:40
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for catalogue_deals
-- ----------------------------
CREATE TABLE `catalogue_deals` (
  `id` int(10) NOT NULL,
  `tid` int(10) NOT NULL,
  `amount` int(10) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for catalogue_items
-- ----------------------------
CREATE TABLE `catalogue_items` (
  `tid` int(10) NOT NULL auto_increment,
  `name_cct` varchar(110) collate latin1_general_ci NOT NULL,
  `typeid` tinyint(1) NOT NULL,
  `length` tinyint(2) NOT NULL,
  `width` tinyint(2) NOT NULL,
  `top` double(4,2) NOT NULL,
  `colour` varchar(100) collate latin1_general_ci NOT NULL,
  `door` int(3) NOT NULL,
  `tradeable` int(1) NOT NULL,
  `recycleable` int(1) NOT NULL,
  `catalogue_id_page` tinyint(3) NOT NULL,
  `catalogue_id_index` int(5) NOT NULL,
  `catalogue_name` varchar(100) collate latin1_general_ci NOT NULL,
  `catalogue_description` varchar(200) collate latin1_general_ci NOT NULL,
  `catalogue_cost` int(5) NOT NULL,
  PRIMARY KEY  (`tid`)
) ENGINE=MyISAM AUTO_INCREMENT=1001 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for catalogue_pages
-- ----------------------------
CREATE TABLE `catalogue_pages` (
  `indexid` tinyint(3) NOT NULL,
  `minrank` tinyint(1) NOT NULL,
  `indexname` varchar(100) collate latin1_general_ci NOT NULL,
  `displayname` varchar(100) collate latin1_general_ci NOT NULL,
  `style_layout` varchar(100) collate latin1_general_ci NOT NULL,
  `img_header` varchar(100) collate latin1_general_ci default NULL,
  `img_side` text collate latin1_general_ci,
  `label_description` text collate latin1_general_ci,
  `label_misc` text collate latin1_general_ci,
  `label_moredetails` varchar(150) collate latin1_general_ci default NULL,
  `opt_bodyreplace` text collate latin1_general_ci,
  PRIMARY KEY  (`indexid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for cms_alerts
-- ----------------------------
CREATE TABLE `cms_alerts` (
  `username` varchar(25) collate latin1_general_ci NOT NULL COMMENT 'User to alert',
  `alert` mediumtext collate latin1_general_ci NOT NULL COMMENT 'Message',
  `pending` enum('0','1') collate latin1_general_ci NOT NULL COMMENT 'Alerted yet?'
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_help
-- ----------------------------
CREATE TABLE `cms_help` (
  `id` int(11) NOT NULL auto_increment,
  `username` varchar(25) collate latin1_general_ci NOT NULL,
  `ip` varchar(50) collate latin1_general_ci NOT NULL,
  `message` mediumtext collate latin1_general_ci NOT NULL,
  `date` varchar(50) collate latin1_general_ci NOT NULL,
  `picked_up` enum('0','1') collate latin1_general_ci NOT NULL,
  `subject` varchar(50) collate latin1_general_ci NOT NULL,
  `roomid` int(20) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_news
-- ----------------------------
CREATE TABLE `cms_news` (
  `num` tinyint(4) NOT NULL,
  `title` text collate latin1_general_ci NOT NULL,
  `category` text collate latin1_general_ci NOT NULL,
  `topstory` varchar(100) collate latin1_general_ci NOT NULL,
  `short_story` text collate latin1_general_ci NOT NULL,
  `story` longtext collate latin1_general_ci NOT NULL,
  `date` date NOT NULL,
  `author` text collate latin1_general_ci NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_posts
-- ----------------------------
CREATE TABLE `cms_posts` (
  `id` int(11) NOT NULL auto_increment,
  `thread` int(11) NOT NULL,
  `poster` varchar(25) NOT NULL,
  `date` varchar(25) NOT NULL,
  `post` text NOT NULL,
  `modified` varchar(50) NOT NULL,
  `modified_by` varchar(25) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_system
-- ----------------------------
CREATE TABLE `cms_system` (
  `sitename` varchar(30) collate latin1_general_ci NOT NULL,
  `shortname` varchar(30) collate latin1_general_ci NOT NULL,
  `site_closed` enum('0','1') collate latin1_general_ci NOT NULL COMMENT 'Maintenance Mode',
  `enable_sso` enum('0','1') collate latin1_general_ci NOT NULL,
  `language` varchar(2) collate latin1_general_ci NOT NULL,
  `ip` varchar(50) collate latin1_general_ci NOT NULL,
  `port` varchar(5) collate latin1_general_ci NOT NULL,
  `texts` varchar(250) collate latin1_general_ci NOT NULL,
  `variables` varchar(250) collate latin1_general_ci NOT NULL,
  `dcr` varchar(250) collate latin1_general_ci NOT NULL,
  `reload_url` varchar(250) collate latin1_general_ci NOT NULL,
  `localhost` enum('0','1') collate latin1_general_ci NOT NULL COMMENT 'Local server?'
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_tags
-- ----------------------------
CREATE TABLE `cms_tags` (
  `id` int(255) NOT NULL auto_increment,
  `owner` varchar(25) collate latin1_general_ci NOT NULL,
  `tag` varchar(25) collate latin1_general_ci NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=9 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for cms_threads
-- ----------------------------
CREATE TABLE `cms_threads` (
  `id` int(11) NOT NULL auto_increment,
  `title` varchar(20) NOT NULL,
  `views` varchar(4) NOT NULL,
  `replies` varchar(4) NOT NULL,
  `starter` varchar(25) NOT NULL,
  `lastposter` varchar(25) NOT NULL,
  `lastpostdate` varchar(25) NOT NULL,
  `lastposttime` varchar(50) NOT NULL,
  `startdate` varchar(25) NOT NULL,
  `unix` varchar(50) NOT NULL,
  `firstpostid` int(20) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COMMENT='HoloCMS';

-- ----------------------------
-- Table structure for furniture
-- ----------------------------
CREATE TABLE `furniture` (
  `id` int(10) NOT NULL auto_increment,
  `tid` int(10) NOT NULL,
  `ownerid` int(10) NOT NULL,
  `roomid` int(10) NOT NULL,
  `x` smallint(6) NOT NULL,
  `y` smallint(6) NOT NULL,
  `z` smallint(6) NOT NULL,
  `h` double(4,2) NOT NULL default '0.00',
  `opt_var` varchar(25) collate latin1_general_ci default NULL,
  `opt_wallpos` varchar(200) collate latin1_general_ci default NULL,
  `opt_teleportid` int(10) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=204 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for furniture_moodlight
-- ----------------------------
CREATE TABLE `furniture_moodlight` (
  `id` int(11) NOT NULL,
  `roomid` int(11) NOT NULL,
  `preset_cur` int(1) NOT NULL default '0',
  `preset_1` varchar(75) collate latin1_general_ci default NULL,
  `preset_2` varchar(75) collate latin1_general_ci default NULL,
  `preset_3` varchar(75) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`id`,`roomid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for furniture_presents
-- ----------------------------
CREATE TABLE `furniture_presents` (
  `id` int(10) NOT NULL,
  `itemid` int(10) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for furniture_recycler
-- ----------------------------
CREATE TABLE `furniture_recycler` (
  `userid` int(10) NOT NULL,
  `itemid` int(11) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for guestroom_modeldata
-- ----------------------------
CREATE TABLE `guestroom_modeldata` (
  `model` char(1) collate latin1_general_ci NOT NULL,
  `door` varchar(50) collate latin1_general_ci default NULL,
  `map_height` text collate latin1_general_ci,
  PRIMARY KEY  (`model`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for guestroom_rights
-- ----------------------------
CREATE TABLE `guestroom_rights` (
  `userid` int(10) NOT NULL,
  `roomid` int(11) NOT NULL,
  KEY `index` (`userid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for guestroom_votes
-- ----------------------------
CREATE TABLE `guestroom_votes` (
  `userid` int(11) NOT NULL,
  `roomid` int(11) NOT NULL,
  `vote` int(2) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for guestrooms
-- ----------------------------
CREATE TABLE `guestrooms` (
  `id` int(10) NOT NULL auto_increment,
  `name` varchar(30) collate latin1_general_ci NOT NULL,
  `owner` varchar(30) collate latin1_general_ci NOT NULL,
  `descr` varchar(100) collate latin1_general_ci default NULL,
  `model` int(2) NOT NULL,
  `category_in` int(3) NOT NULL default '0',
  `wallpaper` int(3) NOT NULL default '0',
  `floor` int(3) NOT NULL default '0',
  `state` int(1) NOT NULL default '0',
  `superusers` int(1) NOT NULL default '0',
  `opt_password` varchar(15) collate latin1_general_ci default NULL,
  `showname` int(1) NOT NULL default '1',
  `incnt_now` int(2) default '0',
  `incnt_max` int(2) default '25',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=119 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for messenger_friendrequests
-- ----------------------------
CREATE TABLE `messenger_friendrequests` (
  `userid_from` int(10) NOT NULL default '0',
  `userid_to` int(10) NOT NULL default '0',
  `requestid` int(10) NOT NULL default '0',
  PRIMARY KEY  (`userid_from`,`userid_to`,`requestid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for messenger_friendships
-- ----------------------------
CREATE TABLE `messenger_friendships` (
  `userid` bigint(20) NOT NULL,
  `friendid` bigint(20) NOT NULL,
  KEY `index extreme` (`userid`),
  KEY `extreme the 2nd` (`friendid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for messenger_messages
-- ----------------------------
CREATE TABLE `messenger_messages` (
  `userid` int(15) NOT NULL,
  `friendid` int(15) NOT NULL,
  `messageid` int(11) NOT NULL,
  `sent_on` text collate latin1_general_ci NOT NULL,
  `message` text collate latin1_general_ci NOT NULL,
  PRIMARY KEY  (`userid`,`messageid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for nav_categories
-- ----------------------------
CREATE TABLE `nav_categories` (
  `id` int(3) NOT NULL default '0',
  `parent` int(3) NOT NULL default '0',
  `name` varchar(100) collate latin1_general_ci NOT NULL default 'Non-named category',
  `ispubcat` int(1) NOT NULL default '1',
  `minrank` int(2) NOT NULL,
  `minrank_hidefor` int(1) NOT NULL,
  `trading` int(1) NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for nav_favrooms
-- ----------------------------
CREATE TABLE `nav_favrooms` (
  `userid` int(10) NOT NULL,
  `roomid` int(10) NOT NULL,
  `ispublicroom` int(1) default '0'
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for publicrooms
-- ----------------------------
CREATE TABLE `publicrooms` (
  `id` int(3) NOT NULL auto_increment,
  `category_in` int(2) NOT NULL default '3',
  `name_caption` varchar(100) collate latin1_general_ci NOT NULL default '???',
  `name_cct` varchar(100) collate latin1_general_ci default NULL,
  `name_icon` varchar(100) collate latin1_general_ci default NULL,
  `name_ae` varchar(100) collate latin1_general_ci default NULL,
  `incnt_now` int(4) NOT NULL,
  `incnt_max` int(4) NOT NULL default '30',
  `map_height` text collate latin1_general_ci,
  `map_items` text collate latin1_general_ci,
  `map_door` varchar(15) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=101 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for rank_fuserights
-- ----------------------------
CREATE TABLE `rank_fuserights` (
  `id` int(3) NOT NULL auto_increment,
  `minrank` int(1) NOT NULL,
  `fuseright` varchar(100) collate latin1_general_ci NOT NULL,
  PRIMARY KEY  (`id`,`fuseright`)
) ENGINE=MyISAM AUTO_INCREMENT=35 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for ranks
-- ----------------------------
CREATE TABLE `ranks` (
  `rankid` int(1) NOT NULL,
  `rankname` varchar(15) collate latin1_general_ci default NULL,
  `ignoreFilter` int(1) NOT NULL,
  `receiveCFH` int(1) NOT NULL,
  `enterAllRooms` int(1) NOT NULL,
  `seeAllOwners` int(1) NOT NULL,
  `adminCatalogue` int(1) NOT NULL,
  `staffFloor` int(1) NOT NULL,
  `rightsEverywhere` int(4) NOT NULL,
  PRIMARY KEY  (`rankid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for roombots
-- ----------------------------
CREATE TABLE `roombots` (
  `id` int(5) NOT NULL,
  `roomid` int(11) NOT NULL,
  `name` varchar(20) collate latin1_general_ci NOT NULL,
  `figure` varchar(255) collate latin1_general_ci NOT NULL,
  `mission` varchar(150) collate latin1_general_ci default NULL,
  `location` varchar(12) collate latin1_general_ci NOT NULL,
  `movements` text collate latin1_general_ci NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for sslevels
-- ----------------------------
CREATE TABLE `sslevels` (
  `id` int(3) NOT NULL auto_increment,
  `catparent` int(2) NOT NULL default '3',
  `name_caption` varchar(100) collate latin1_general_ci NOT NULL default '???',
  `name_cct` varchar(100) collate latin1_general_ci default NULL,
  `name_icon` varchar(100) collate latin1_general_ci default NULL,
  `name_ae` varchar(100) collate latin1_general_ci default NULL,
  `incnt_now` int(4) NOT NULL,
  `incnt_max` int(4) NOT NULL default '30',
  `map_height` text collate latin1_general_ci,
  `map_items` text collate latin1_general_ci,
  `map_door` varchar(15) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=101 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for system
-- ----------------------------
CREATE TABLE `system` (
  `onlinecount` int(3) NOT NULL default '0',
  `test` text collate latin1_general_ci
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for system_config
-- ----------------------------
CREATE TABLE `system_config` (
  `skey` varchar(100) collate latin1_general_ci NOT NULL,
  `sval` text collate latin1_general_ci NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for system_recycler
-- ----------------------------
CREATE TABLE `system_recycler` (
  `rclr_cost` int(5) NOT NULL,
  `rclr_reward` int(10) NOT NULL,
  PRIMARY KEY  (`rclr_cost`,`rclr_reward`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for system_stafflog
-- ----------------------------
CREATE TABLE `system_stafflog` (
  `id` int(5) NOT NULL auto_increment,
  `action` varchar(12) collate latin1_general_ci NOT NULL,
  `message` text collate latin1_general_ci,
  `note` text collate latin1_general_ci,
  `userid` int(11) NOT NULL,
  `targetid` int(11) default NULL,
  `timestamp` varchar(50) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=7 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for system_strings
-- ----------------------------
CREATE TABLE `system_strings` (
  `stringid` varchar(100) collate latin1_general_ci NOT NULL default 'null',
  `var_en` text collate latin1_general_ci,
  `var_de` text collate latin1_general_ci,
  PRIMARY KEY  (`stringid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for users
-- ----------------------------
CREATE TABLE `users` (
  `id` int(15) NOT NULL default '0',
  `name` varchar(50) collate latin1_general_ci default NULL,
  `password` varchar(100) collate latin1_general_ci default NULL,
  `rank` int(1) default NULL,
  `email` varchar(100) collate latin1_general_ci default NULL,
  `birth` varchar(100) collate latin1_general_ci default NULL,
  `hbirth` varchar(100) collate latin1_general_ci default NULL,
  `figure` varchar(150) collate latin1_general_ci default NULL,
  `sex` varchar(5) collate latin1_general_ci default NULL,
  `mission` varchar(50) collate latin1_general_ci default NULL,
  `consolemission` varchar(50) collate latin1_general_ci default NULL,
  `credits` int(7) default NULL,
  `tickets` smallint(5) default NULL,
  `badgestatus` varchar(5) collate latin1_general_ci default NULL,
  `lastvisit` varchar(50) collate latin1_general_ci default NULL,
  `user` text collate latin1_general_ci,
  `postcount` bigint(20) NOT NULL default '0',
  `ticket_sso` varchar(39) collate latin1_general_ci default NULL,
  `ipaddress_last` varchar(100) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for users_badges
-- ----------------------------
CREATE TABLE `users_badges` (
  `userid` int(15) NOT NULL,
  `badgeid` varchar(5) collate latin1_general_ci NOT NULL default '',
  PRIMARY KEY  (`userid`,`badgeid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for users_bans
-- ----------------------------
CREATE TABLE `users_bans` (
  `userid` varchar(30) collate latin1_general_ci NOT NULL default '',
  `ipaddress` varchar(100) collate latin1_general_ci default NULL,
  `date_expire` varchar(50) collate latin1_general_ci default NULL,
  `descr` text collate latin1_general_ci,
  PRIMARY KEY  (`userid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for users_club
-- ----------------------------
CREATE TABLE `users_club` (
  `userid` bigint(6) NOT NULL,
  `months_expired` int(4) default NULL,
  `months_left` int(4) default NULL,
  `date_monthstarted` varchar(25) collate latin1_general_ci default NULL,
  PRIMARY KEY  (`userid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for users_recycler
-- ----------------------------
CREATE TABLE `users_recycler` (
  `userid` int(10) NOT NULL,
  `session_started` varchar(100) collate latin1_general_ci NOT NULL,
  `session_reward` int(10) NOT NULL,
  PRIMARY KEY  (`userid`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for vouchers
-- ----------------------------
CREATE TABLE `vouchers` (
  `voucher` varchar(20) collate latin1_general_ci NOT NULL default '',
  `credits` int(5) default NULL,
  PRIMARY KEY  (`voucher`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Table structure for wordfilter
-- ----------------------------
CREATE TABLE `wordfilter` (
  `word` varchar(100) collate latin1_general_ci NOT NULL,
  PRIMARY KEY  (`word`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;

-- ----------------------------
-- Records 
-- ----------------------------
INSERT INTO `catalogue_deals` VALUES ('1', '1', '6');
INSERT INTO `catalogue_deals` VALUES ('1', '419', '3');
INSERT INTO `catalogue_deals` VALUES ('1', '471', '5');
INSERT INTO `catalogue_deals` VALUES ('1', '398', '1');
INSERT INTO `catalogue_items` VALUES ('1', 'rare_parasol*0', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#94f718', '0', '1', '1', '6', '1', 'Green Parasol', 'Block those rays!', '25');
INSERT INTO `catalogue_items` VALUES ('2', 'floor', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '2', 'floor', 'Floor', '3');
INSERT INTO `catalogue_items` VALUES ('3', 'wallpaper 1', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '3', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('4', 'wallpaper 2', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '4', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('5', 'wallpaper 3', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '5', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('6', 'wallpaper 4', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '6', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('7', 'wallpaper 5', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '7', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('8', 'wallpaper 6', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '8', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('9', 'wallpaper 7', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '9', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('10', 'wallpaper 8', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '10', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('11', 'wallpaper 9', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '11', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('12', 'wallpaper 10', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '12', 'wallpaper', 'wallpaper', '3');
INSERT INTO `catalogue_items` VALUES ('13', 'wallpaper 11', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '13', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('14', 'wallpaper 12', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '14', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('15', 'wallpaper 13', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '15', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('16', 'wallpaper 14', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '16', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('17', 'wallpaper 15', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '17', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('18', 'wallpaper 16', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '18', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('19', 'wallpaper 17', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '19', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('20', 'wallpaper 18', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '20', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('21', 'wallpaper 19', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '21', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('22', 'wallpaper 20', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '22', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('23', 'wallpaper 21', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '23', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('24', 'wallpaper 22', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '24', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('25', 'wallpaper 23', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '25', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('26', 'wallpaper 24', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '26', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('27', 'wallpaper 25', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '27', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('28', 'wallpaper 26', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '28', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('29', 'wallpaper 27', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '29', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('30', 'wallpaper 28', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '30', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('31', 'wallpaper 29', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '31', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('32', 'wallpaper 30', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '32', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('33', 'wallpaper 31', '0', '0', '0', '0.00', '0', '0', '0', '0', '7', '33', 'wallpaper', 'wallpaper', '5');
INSERT INTO `catalogue_items` VALUES ('34', 'nest', '1', '1', '1', '1.00', '0,0,0', '0', '0', '1', '8', '34', 'Basket', 'Night, night', '20');
INSERT INTO `catalogue_items` VALUES ('35', 'nest', '1', '1', '1', '1.00', '0,0,0', '0', '0', '1', '8', '35', 'Basket', 'Night, night', '20');
INSERT INTO `catalogue_items` VALUES ('36', 'nest', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '8', '36', 'Basket', 'Night, night', '20');
INSERT INTO `catalogue_items` VALUES ('37', 'deal97', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '0', '37', 'Doggy Bones', 'Natural nutrition for the barking one', '2');
INSERT INTO `catalogue_items` VALUES ('38', 'deal98', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '0', '38', 'Sardines', 'Fresh catch of the day', '2');
INSERT INTO `catalogue_items` VALUES ('39', 'deal99', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '0', '39', 'Cabbage', 'Health food for pets', '2');
INSERT INTO `catalogue_items` VALUES ('40', 'deal96', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '0', '40', 'T-Bones', 'For the croc!', '2');
INSERT INTO `catalogue_items` VALUES ('41', 'petfood1', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '9', '41', 'Doggy Bones', 'Natural nutrition for the barking one', '1');
INSERT INTO `catalogue_items` VALUES ('42', 'petfood2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '9', '42', 'Sardines', 'Fresh catch of the day', '1');
INSERT INTO `catalogue_items` VALUES ('43', 'petfood3', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '9', '43', 'Cabbage', 'Health food for pets', '1');
INSERT INTO `catalogue_items` VALUES ('44', 'petfood4', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '9', '44', 'T-Bones', 'For the croc!', '1');
INSERT INTO `catalogue_items` VALUES ('45', 'waterbowl*1', '1', '1', '1', '1.00', '#ff3f3f,#ffffff,#ffffff', '0', '1', '1', '9', '45', 'Red Water Bowl', 'Aqua unlimited', '2');
INSERT INTO `catalogue_items` VALUES ('46', 'waterbowl*2', '1', '1', '1', '1.00', '#3fff3f,#ffffff,#ffffff', '0', '1', '1', '9', '46', 'Green Water Bowl', 'Aqua unlimited', '2');
INSERT INTO `catalogue_items` VALUES ('47', 'waterbowl*3', '1', '1', '1', '1.00', '#ffff00,#ffffff,#ffffff', '0', '1', '1', '9', '47', 'Yellow Water Bowl', 'Aqua unlimited', '2');
INSERT INTO `catalogue_items` VALUES ('48', 'waterbowl*4', '1', '1', '1', '1.00', '#0099ff,#ffffff,#ffffff', '0', '1', '1', '9', '48', 'Blue Water Bowl', 'Aqua unlimited', '2');
INSERT INTO `catalogue_items` VALUES ('49', 'waterbowl*5', '1', '1', '1', '1.00', '#bf7f00,#ffffff,#ffffff', '0', '1', '1', '9', '49', 'Brown Water Bowl', 'Aqua unlimited', '2');
INSERT INTO `catalogue_items` VALUES ('50', 'goodie2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '9', '50', 'Chocolate Mouse', 'For gourmet kittens', '1');
INSERT INTO `catalogue_items` VALUES ('51', 'goodie1', '1', '1', '1', '1.00', '#ff4cbf,#ffffff', '0', '1', '1', '9', '51', 'Marzipan Man', 'Crunchy Dog Treat', '1');
INSERT INTO `catalogue_items` VALUES ('52', 'goodie1*1', '1', '1', '1', '1.00', '#3fffff,#ffffff', '0', '1', '1', '9', '52', 'Marzipan Man', 'Crunchy Dog Treat', '1');
INSERT INTO `catalogue_items` VALUES ('53', 'goodie1*2', '1', '1', '1', '1.00', '#ffbf00,#ffffff', '0', '1', '1', '9', '53', 'Marzipan Man', 'Crunchy Dog Treat', '1');
INSERT INTO `catalogue_items` VALUES ('54', 'toy1', '1', '1', '1', '1.00', '#ff0000,#ffff00,#ffffff,#ffffff', '0', '1', '1', '9', '54', 'Rubber Ball', 'it\'s bouncy-tastic', '2');
INSERT INTO `catalogue_items` VALUES ('55', 'toy1*1', '1', '1', '1', '1.00', '#ff7f00,#007f00,#ffffff,#ffffff', '0', '1', '1', '9', '55', 'Rubber Ball', 'it\'s bouncy-tastic', '2');
INSERT INTO `catalogue_items` VALUES ('56', 'toy1*2', '1', '1', '1', '1.00', '#003f7f,#ff00bf,#ffffff,#ffffff', '0', '1', '1', '9', '56', 'Rubber Ball', 'it\'s bouncy-tastic', '2');
INSERT INTO `catalogue_items` VALUES ('57', 'toy1*3', '1', '1', '1', '1.00', '#bf1900,#00bfbf,#ffffff,#ffffff', '0', '1', '1', '9', '57', 'Rubber Ball', 'it\'s bouncy-tastic', '2');
INSERT INTO `catalogue_items` VALUES ('58', 'toy1*4', '1', '1', '1', '1.00', '#000000,#ffffff,#ffffff,#ffffff', '0', '1', '1', '9', '58', 'Rubber Ball', 'it\'s bouncy-tastic', '2');
INSERT INTO `catalogue_items` VALUES ('59', 'silo_studydesk', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '10', '59', 'Area Quest Desk', 'For the true Habbo Scholars', '15');
INSERT INTO `catalogue_items` VALUES ('60', 'bed_silo_two', '2', '2', '3', '1.00', '0,0,0', '0', '1', '1', '10', '60', 'Double Bed', 'Plain and simple x2', '3');
INSERT INTO `catalogue_items` VALUES ('61', 'bed_silo_one', '2', '1', '3', '1.00', '0,0,0', '0', '1', '1', '10', '61', 'Single Bed', 'Plain and simple', '3');
INSERT INTO `catalogue_items` VALUES ('62', 'shelves_silo', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '10', '62', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('63', 'sofa_silo', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#A2C6C8,#A2C6C8,#A2C6C8,#A2C6C8', '0', '1', '1', '10', '63', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('64', 'sofachair_silo', '1', '1', '1', '1.00', '#ffffff,#ffffff,#A2C6C8,#A2C6C8,#ffffff', '0', '1', '1', '10', '64', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('65', 'table_silo_small', '1', '1', '1', '1.00', '#ffffff,#A2C6C8,#ffffff,#A2C6C8', '0', '1', '1', '10', '65', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('66', 'divider_silo3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#A2C6C8', '1', '1', '1', '10', '66', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('67', 'divider_silo2', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '10', '67', 'Screen', 'Stylish sectioning', '3');
INSERT INTO `catalogue_items` VALUES ('68', 'divider_silo1', '1', '1', '1', '1.00', '#ffffff,#A2C6C8,0,#A2C6C8', '0', '1', '1', '10', '68', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('69', 'chair_silo', '2', '1', '1', '1.00', '#ffffff,#ffffff,#A2C6C8,#A2C6C8,#ffffff', '0', '1', '1', '10', '69', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('70', 'safe_silo', '1', '1', '1', '1.00', '#ffffff,#A2C6C8,#ffffff', '0', '1', '1', '10', '70', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('71', 'barchair_silo', '1', '1', '1', '1.00', '#ffffff,#A2C6C8,#ffffff', '0', '1', '1', '10', '71', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('72', 'table_silo_med', '1', '2', '2', '1.00', '#ffffff,#8FAEAF', '0', '1', '1', '10', '72', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('73', 'tv_luxus', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '11', '73', 'Digital TV', 'Bang up to date', '6');
INSERT INTO `catalogue_items` VALUES ('74', 'wood_tv', '1', '1', '2', '1.00', '0,0,0', '0', '1', '1', '11', '74', 'Large TV', 'For family viewing', '4');
INSERT INTO `catalogue_items` VALUES ('75', 'red_tv', '1', '1', '1', '1.50', '0,0,0', '0', '1', '1', '11', '75', 'Portable TV', 'Don\'t miss those soaps', '3');
INSERT INTO `catalogue_items` VALUES ('76', 'post.it', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '11', '76', 'Pad of stickies', 'Pad of stickies', '3');
INSERT INTO `catalogue_items` VALUES ('77', 'pizza', '1', '1', '1', '1.60', '0,0,0', '0', '1', '1', '11', '77', 'Pizza Box', 'You dirty Habbo', '3');
INSERT INTO `catalogue_items` VALUES ('78', 'drinks', '1', '1', '1', '1.50', '0,0,0', '0', '1', '1', '11', '78', 'Empty Cans', 'Are you a slob too?', '3');
INSERT INTO `catalogue_items` VALUES ('79', 'bottle', '1', '1', '1', '1.50', '0,0,0', '0', '1', '1', '11', '79', 'Empty Spinning Bottle', 'For interesting games!', '3');
INSERT INTO `catalogue_items` VALUES ('80', 'edice', '1', '1', '1', '1.50', '0,0,0', '0', '1', '1', '11', '80', 'Holo-dice', 'What\'s your lucky number?', '6');
INSERT INTO `catalogue_items` VALUES ('81', 'habbocake', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '81', 'Cake', 'Save me a slice!', '4');
INSERT INTO `catalogue_items` VALUES ('82', 'menorah', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '82', 'Menorah', 'Light up your room', '3');
INSERT INTO `catalogue_items` VALUES ('83', 'bunny', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '83', 'Squidgy Bunny', 'Yours to cuddle up to', '3');
INSERT INTO `catalogue_items` VALUES ('84', 'poster 44', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '11', '84', 'Mummy', 'Beware the curse...', '3');
INSERT INTO `catalogue_items` VALUES ('85', 'wcandleset', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '85', 'White Candle Plate', 'Simple but stylish', '3');
INSERT INTO `catalogue_items` VALUES ('86', 'rcandleset', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '86', 'Red Candle Plate', 'Simple but stylish', '3');
INSERT INTO `catalogue_items` VALUES ('87', 'ham', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '87', 'Joint of Ham', 'Tuck in', '3');
INSERT INTO `catalogue_items` VALUES ('88', 'hockey_light', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '11', '88', 'Lert', 'Set it off.', '5');
INSERT INTO `catalogue_items` VALUES ('89', 'wall_china', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '12', '89', 'Drakenscherm', 'Voor je eigen Grote Muur', '8');
INSERT INTO `catalogue_items` VALUES ('90', 'corner_china', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '12', '90', 'Voor Grote Muur', 'Stevig en vuurvast', '8');
INSERT INTO `catalogue_items` VALUES ('91', 'china_shelve', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '12', '91', 'Lakwerk boekenkast', 'Bewaarder van je boekenschat', '8');
INSERT INTO `catalogue_items` VALUES ('92', 'china_table', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '12', '92', 'Yin Yang tafel', 'Exotic and classy', '5');
INSERT INTO `catalogue_items` VALUES ('93', 'chair_china', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '12', '93', 'Yin Yang stoel', 'Zitten in evenwicht', '5');
INSERT INTO `catalogue_items` VALUES ('94', 'poster 57', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '12', '94', 'Kalligrafie', 'Een kwast, vaste hand en concentratie', '3');
INSERT INTO `catalogue_items` VALUES ('95', 'poster 58', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '12', '95', 'Chinese rode knopen', 'Veel geluk!', '5');
INSERT INTO `catalogue_items` VALUES ('96', 'cn_sofa', '2', '3', '1', '1.00', '0,0,0', '0', '1', '1', '12', '96', 'Yin Yang bank', 'Zitten in evenwicht!', '10');
INSERT INTO `catalogue_items` VALUES ('97', 'cn_lamp', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '12', '97', 'Rode lantaarn', 'Zacht licht voor je Habbo Kamer', '15');
INSERT INTO `catalogue_items` VALUES ('98', 'bath', '2', '1', '2', '1.00', '0,0,0', '0', '1', '1', '13', '98', 'Bubble Bath', 'The ultimate in pampering', '6');
INSERT INTO `catalogue_items` VALUES ('99', 'sink', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '13', '99', 'Sink', 'Hot and cold thrown in for no charge', '3');
INSERT INTO `catalogue_items` VALUES ('100', 'duck', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '13', '100', 'Rubber Duck', 'Every bather needs one', '1');
INSERT INTO `catalogue_items` VALUES ('101', 'toilet', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '13', '101', 'Loo Seat', 'Loo Seat', '4');
INSERT INTO `catalogue_items` VALUES ('102', 'toilet_red', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '13', '102', 'Loo Seat', 'Loo Seat', '4');
INSERT INTO `catalogue_items` VALUES ('103', 'toilet_yell', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '13', '103', 'Loo Seat', 'Loo Seat', '4');
INSERT INTO `catalogue_items` VALUES ('104', 'tile', '4', '4', '4', '1.00', '0,0,0', '0', '1', '1', '13', '104', 'Floor Tiles', 'In a choice of colours', '3');
INSERT INTO `catalogue_items` VALUES ('105', 'tile_red', '4', '4', '4', '1.00', '0,0,0', '0', '1', '1', '13', '105', 'Floor Tiles', 'In a choice of colours', '3');
INSERT INTO `catalogue_items` VALUES ('106', 'tile_yell', '4', '4', '4', '1.00', '0,0,0', '0', '1', '1', '13', '106', 'Floor Tiles', 'In a choice of colours', '3');
INSERT INTO `catalogue_items` VALUES ('107', 'bardesk_polyfon*5', '1', '2', '1', '1.00', '#ffffff,#ffffff,#FF9BBD,#FF9BBD', '0', '1', '1', '14', '107', 'Candy Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('108', 'bardeskcorner_polyfon*5', '1', '1', '1', '1.00', '#ffffff,#FF9BBD', '0', '1', '1', '14', '108', 'Candy Corner', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('109', 'divider_poly3*5', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '14', '109', 'Candy Hatch (lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('110', 'sofachair_polyfon_girl', '2', '1', '1', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '14', '110', 'Armchair', 'Think pink', '3');
INSERT INTO `catalogue_items` VALUES ('111', 'sofa_polyfon_girl', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#EE7EA4,#EE7EA4,#E', '0', '1', '1', '14', '111', 'Two-seater Sofa', 'Romantic pink for two', '4');
INSERT INTO `catalogue_items` VALUES ('112', 'carpet_polar*1', '4', '2', '3', '0.00', '#ffbbcf,#ffbbcf,#ffddef', '0', '1', '1', '14', '112', 'Pink Faux-Fur', 'Bear Rug Cute', '4');
INSERT INTO `catalogue_items` VALUES ('113', 'bed_polyfon_girl_one', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '14', '113', 'Armchair', 'Think pink', '3');
INSERT INTO `catalogue_items` VALUES ('114', 'bed_polyfon_girl', '2', '2', '3', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '14', '114', 'Double Bed', 'Snuggle down in princess pink', '4');
INSERT INTO `catalogue_items` VALUES ('115', 'camera', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '15', '115', 'Camera', 'Smile!', '10');
INSERT INTO `catalogue_items` VALUES ('116', 'photo_film', '0', '0', '0', '0.00', '0,0,0', '0', '0', '0', '15', '116', 'Film', 'Film for five photos', '6');
INSERT INTO `catalogue_items` VALUES ('117', 'poster 500', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '117', 'Union Jack', 'The UK flag', '3');
INSERT INTO `catalogue_items` VALUES ('118', 'poster 501', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '118', 'Jolly Roger', 'For pirates everywhere', '3');
INSERT INTO `catalogue_items` VALUES ('119', 'poster 502', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '119', 'The Stars and Stripes', 'The US flag', '3');
INSERT INTO `catalogue_items` VALUES ('120', 'poster 503', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '120', 'The Swiss flag', 'There\'s no holes in this...', '3');
INSERT INTO `catalogue_items` VALUES ('121', 'poster 504', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '121', 'The Bundesflagge', 'The German flag', '3');
INSERT INTO `catalogue_items` VALUES ('122', 'poster 505', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '122', 'The Maple Leaf', 'The Canadian flag', '3');
INSERT INTO `catalogue_items` VALUES ('123', 'poster 506', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '123', 'The flag of Finland', 'To \'Finnish\' your decor...', '3');
INSERT INTO `catalogue_items` VALUES ('124', 'poster 507', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '124', 'The French Tricolore', 'The French flag', '3');
INSERT INTO `catalogue_items` VALUES ('125', 'poster 508', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '125', 'The Spanish flag', 'The flag of Spain', '3');
INSERT INTO `catalogue_items` VALUES ('126', 'poster 509', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '126', 'The Jamaican flag', 'The flag of Jamaica', '3');
INSERT INTO `catalogue_items` VALUES ('127', 'poster 510', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '127', 'The Italian flag', 'The flag of Italy', '3');
INSERT INTO `catalogue_items` VALUES ('128', 'poster 511', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '128', 'The Dutch flag', 'The flag of The Netherlands', '3');
INSERT INTO `catalogue_items` VALUES ('129', 'poster 512', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '129', 'The Irish flag', 'The flag of Ireland', '3');
INSERT INTO `catalogue_items` VALUES ('130', 'poster 513', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '130', 'The Australian flag', 'Aussies rule!', '3');
INSERT INTO `catalogue_items` VALUES ('131', 'poster 514', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '131', 'The EU flag', 'Be proud to be in the Union!', '3');
INSERT INTO `catalogue_items` VALUES ('132', 'poster 515', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '132', 'The Swedish flag', 'Waved by Swedes everywhere', '3');
INSERT INTO `catalogue_items` VALUES ('133', 'poster 516', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '133', 'The English flag', 'Eng-er-land', '3');
INSERT INTO `catalogue_items` VALUES ('134', 'poster 517', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '134', 'The Scottish flag', 'Where\'s your kilt?', '3');
INSERT INTO `catalogue_items` VALUES ('135', 'poster 518', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '135', 'The Welsh flag', 'A fiery dragon for your wall', '3');
INSERT INTO `catalogue_items` VALUES ('136', 'poster 520', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '136', 'The Rainbow Flag', 'Every colour for everyone', '3');
INSERT INTO `catalogue_items` VALUES ('137', 'poster 521', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '137', 'Flag of Brazil', 'The flag of Brazil', '3');
INSERT INTO `catalogue_items` VALUES ('138', 'poster 522', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '138', 'The flag of Japan', 'The flag of Japan', '3');
INSERT INTO `catalogue_items` VALUES ('139', 'poster 523', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '16', '139', 'The flag of India', 'The flag of India', '3');
INSERT INTO `catalogue_items` VALUES ('140', 'poster 2001', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '140', 'Rug on the wall', 'Rug', '10');
INSERT INTO `catalogue_items` VALUES ('141', 'poster 2004', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '141', 'Rastaman', 'Rasta tarjoaa rakkautta', '3');
INSERT INTO `catalogue_items` VALUES ('142', 'poster 2002', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '142', 'Presidentin Muotokuva', 'Presidentin Muotokuva', '10');
INSERT INTO `catalogue_items` VALUES ('143', 'poster 7', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '143', 'Hammer Cabinet', 'for emergencies only', '3');
INSERT INTO `catalogue_items` VALUES ('144', 'poster 12', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '144', 'Skyline Poster', 'skyscrapers at night', '3');
INSERT INTO `catalogue_items` VALUES ('145', 'poster 13', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '145', 'BW Skyline Poster', 'skyscrapers in arty black and white', '3');
INSERT INTO `catalogue_items` VALUES ('146', 'poster 14', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '146', 'Fox Poster', 'a truly cunning design', '3');
INSERT INTO `catalogue_items` VALUES ('147', 'poster 17', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '147', 'Butterfly Cabinet', 'beautiful reproduction butterfly', '3');
INSERT INTO `catalogue_items` VALUES ('148', 'poster 18', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '148', 'Butterfly Cabinet (blue)', 'beautiful reproduction butterfly', '3');
INSERT INTO `catalogue_items` VALUES ('149', 'poster 16', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '149', 'Bars', 'high security for your room', '3');
INSERT INTO `catalogue_items` VALUES ('150', 'poster 1003', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '150', 'UK Map', 'get the lovely isles on your walls', '3');
INSERT INTO `catalogue_items` VALUES ('151', 'poster 15', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '151', 'Himalaya Poster', 'marvellous mountains', '3');
INSERT INTO `catalogue_items` VALUES ('152', 'poster 1006', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '152', 'Hoot Poster', 'The eyes follow you...', '3');
INSERT INTO `catalogue_items` VALUES ('153', 'poster 32', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '153', 'Siva Poster', 'The Auspicious One', '3');
INSERT INTO `catalogue_items` VALUES ('154', 'poster 33', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '154', 'Save the Panda', 'We can\'t bear to lose them', '3');
INSERT INTO `catalogue_items` VALUES ('155', 'poster 2', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '155', 'Carrot Plaque', 'take pride in your veg!', '3');
INSERT INTO `catalogue_items` VALUES ('156', 'poster 3', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '156', 'Fish Plaque', 'smells fishy, looks cool', '3');
INSERT INTO `catalogue_items` VALUES ('157', 'poster 4', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '157', 'Bear Plaque', 'fake of course!', '3');
INSERT INTO `catalogue_items` VALUES ('158', 'poster 5', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '158', 'Duck Poster', 'quacking good design!', '3');
INSERT INTO `catalogue_items` VALUES ('159', 'poster 1', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '159', 'Ancestor Poster', 'a touch of history', '3');
INSERT INTO `catalogue_items` VALUES ('160', 'poster 6', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '160', 'Abstract Poster', 'but is it the right way up?', '3');
INSERT INTO `catalogue_items` VALUES ('161', 'poster 1002', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '161', 'Queen Mum Poster', 'aw, bless...', '3');
INSERT INTO `catalogue_items` VALUES ('162', 'poster 1001', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '162', 'Prince Charles Poster', 'Even walls have ears', '3');
INSERT INTO `catalogue_items` VALUES ('163', 'poster 8', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '163', 'Habbo Colours Poster', 'Habbos come in all colours', '3');
INSERT INTO `catalogue_items` VALUES ('164', 'poster 9', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '164', 'Rainforest Poster', 'do your bit for the environment', '3');
INSERT INTO `catalogue_items` VALUES ('165', 'poster 1004', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '165', 'Eid Mubarak', 'Celebrate with us', '3');
INSERT INTO `catalogue_items` VALUES ('166', 'poster 2006', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '166', 'DJ Throne', 'He is the magic Habbo', '3');
INSERT INTO `catalogue_items` VALUES ('167', 'poster 2008', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '167', 'Green Gecko Art', 'Like a gecko it wont fall of your wall', '3');
INSERT INTO `catalogue_items` VALUES ('168', 'poster 11', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '168', 'Certificate', 'award, charter or the Habbo Way - you decide', '3');
INSERT INTO `catalogue_items` VALUES ('169', 'poster 41', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '169', 'Habbo Golden Record', 'For the best music-makers', '3');
INSERT INTO `catalogue_items` VALUES ('170', 'poster 40', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '170', 'Bonnie Blonde', 'The one and only. Adore her!', '3');
INSERT INTO `catalogue_items` VALUES ('171', 'poster 35', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '171', 'The Habbo Babes 1', 'The Hotel\'s girlband. Dream on!', '3');
INSERT INTO `catalogue_items` VALUES ('172', 'poster 36', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '172', 'The Habbo Babes 2', 'The Hotel\'s girlband. Dream on!', '3');
INSERT INTO `catalogue_items` VALUES ('173', 'poster 37', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '173', 'The Habbo Babes 3', 'The Hotel\'s girlband. Dream on!', '3');
INSERT INTO `catalogue_items` VALUES ('174', 'poster 1005', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '174', 'Johnny Squabble', 'The muscly movie hero', '3');
INSERT INTO `catalogue_items` VALUES ('175', 'poster 34', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '175', 'Scamme\'d', 'Habbo-punk for the never-agreeing', '3');
INSERT INTO `catalogue_items` VALUES ('176', 'poster 39', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '176', 'Screaming Furnies', 'The rock masters of virtual music', '3');
INSERT INTO `catalogue_items` VALUES ('177', 'poster 2000', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '177', 'Suomen kartta', 'Suomen kartta', '3');
INSERT INTO `catalogue_items` VALUES ('178', 'poster 38', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '178', 'Smiling Headbangerz', 'For really TOUGH Habbos!', '3');
INSERT INTO `catalogue_items` VALUES ('179', 'poster 2008', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '179', 'Habbo Cola Poster', 'For serious bubblers', '3');
INSERT INTO `catalogue_items` VALUES ('180', 'poster 2007', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '180', 'The Father Of Habbo', 'The legendary founder of the Hotel', '3');
INSERT INTO `catalogue_items` VALUES ('181', 'poster 2003', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '181', 'Dodgy Geezer', 'Would you trust this man?', '3');
INSERT INTO `catalogue_items` VALUES ('182', 'poster 31', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '182', 'System of a Ban', 'Pure and unbridled nu-metal', '3');
INSERT INTO `catalogue_items` VALUES ('183', 'poster 55', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '17', '183', 'Beach Tree Poster', 'Natural', '3');
INSERT INTO `catalogue_items` VALUES ('184', 'glass_shelf', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '18', '184', 'Glass shelf', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('185', 'glass_sofa', '2', '2', '1', '1.00', '#ffffff,#ABD0D2,#ABD0D2,#ffffff,#ffffff,#ABD0D2,#ffffff,#ABD0D2', '0', '1', '1', '18', '185', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('186', 'glass_table', '1', '2', '2', '1.00', '#ffffff,#ABD0D2,#ABD0D2,#ffffff', '0', '1', '1', '18', '186', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('187', 'glass_chair', '2', '1', '1', '1.00', '#ffffff,#ABD0D2,#ABD0D2,#ffffff', '0', '1', '1', '18', '187', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('188', 'glass_stool', '2', '1', '1', '1.00', '#ffffff,#ABD0D2,#ABD0D2,#ffffff', '0', '1', '1', '18', '188', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('189', 'glass_sofa*2', '2', '2', '1', '1.00', '#ffffff,#525252,#525252,#ffffff,#ffffff,#525252,#ffffff,#525252', '0', '1', '1', '18', '189', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('190', 'glass_table*2', '1', '2', '2', '1.00', '#ffffff,#525252,#525252,#ffffff', '0', '1', '1', '18', '190', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('191', 'glass_chair*2', '2', '1', '1', '1.00', '#ffffff,#525252,#525252,#ffffff', '0', '1', '1', '18', '191', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('192', 'glass_stool*2', '2', '1', '1', '1.00', '#ffffff,#525252,#525252,#ffffff', '0', '1', '1', '18', '192', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('193', 'glass_sofa*3', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '18', '193', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('194', 'glass_table*3', '1', '2', '2', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '18', '194', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('195', 'glass_chair*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '18', '195', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('196', 'glass_stool*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '18', '196', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('197', 'glass_sofa*4', '2', '2', '1', '1.00', '#ffffff,#F7EBBC,#F7EBBC,#ffffff,#ffffff,#F7EBBC,#ffffff,#F7EBBC', '0', '1', '1', '18', '197', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('198', 'glass_table*4', '1', '2', '2', '1.00', '#ffffff,#F7EBBC,#F7EBBC,#ffffff', '0', '1', '1', '18', '198', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('199', 'glass_chair*4', '2', '1', '1', '1.00', '#ffffff,#F7EBBC,#F7EBBC,#ffffff', '0', '1', '1', '18', '199', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('200', 'glass_stool*4', '2', '1', '1', '1.00', '#ffffff,#F7EBBC,#F7EBBC,#ffffff', '0', '1', '1', '18', '200', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('201', 'glass_sofa*5', '2', '2', '1', '1.00', '#ffffff,#FF99BC,#FF99BC,#ffffff,#ffffff,#FF99BC,#ffffff,#FF99BC', '0', '1', '1', '18', '201', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('202', 'glass_table*5', '1', '2', '2', '1.00', '#ffffff,#FF99BC,#FF99BC,#ffffff', '0', '1', '1', '18', '202', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('203', 'glass_chair*5', '2', '1', '1', '1.00', '#ffffff,#FF99BC,#FF99BC,#ffffff', '0', '1', '1', '18', '203', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('204', 'glass_stool*5', '2', '1', '1', '1.00', '#ffffff,#FF99BC,#FF99BC,#ffffff', '0', '1', '1', '18', '204', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('205', 'glass_sofa*6', '2', '2', '1', '1.00', '#ffffff,#5EAAF8,#5EAAF8,#ffffff,#ffffff,#5EAAF8,#ffffff,#5EAAF8', '0', '1', '1', '18', '205', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('206', 'glass_table*6', '1', '2', '2', '1.00', '#ffffff,#5EAAF8,#5EAAF8,#ffffff', '0', '1', '1', '18', '206', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('207', 'glass_chair*6', '2', '1', '1', '1.00', '#ffffff,#5EAAF8,#5EAAF8,#ffffff', '0', '1', '1', '18', '207', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('208', 'glass_stool*6', '2', '1', '1', '1.00', '#ffffff,#5EAAF8,#5EAAF8,#ffffff', '0', '1', '1', '18', '208', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('209', 'glass_sofa*7', '2', '2', '1', '1.00', '#ffffff,#92D13D,#92D13D,#ffffff,#ffffff,#92D13D,#ffffff,#92D13D', '0', '1', '1', '18', '209', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('210', 'glass_table*7', '1', '2', '2', '1.00', '#ffffff,#92D13D,#92D13D,#ffffff', '0', '1', '1', '18', '210', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('211', 'glass_chair*7', '2', '1', '1', '1.00', '#ffffff,#92D13D,#92D13D,#ffffff', '0', '1', '1', '18', '211', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('212', 'glass_stool*7', '2', '1', '1', '1.00', '#ffffff,#92D13D,#92D13D,#ffffff', '0', '1', '1', '18', '212', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('213', 'glass_sofa*8', '2', '2', '1', '1.00', '#ffffff,#FFD837,#FFD837,#ffffff,#ffffff,#FFD837,#ffffff,#FFD837', '0', '1', '1', '18', '213', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('214', 'glass_table*8', '1', '2', '2', '1.00', '#ffffff,#FFD837,#FFD837,#ffffff', '0', '1', '1', '18', '214', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('215', 'glass_chair*8', '2', '1', '1', '1.00', '#ffffff,#FFD837,#FFD837,#ffffff', '0', '1', '1', '18', '215', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('216', 'glass_stool*8', '2', '1', '1', '1.00', '#ffffff,#FFD837,#FFD837,#ffffff', '0', '1', '1', '18', '216', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('217', 'glass_sofa*9', '2', '2', '1', '1.00', '#ffffff,#E14218,#E14218,#ffffff,#ffffff,#E14218,#ffffff,#E14218', '0', '1', '1', '18', '217', 'Glass sofa', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('218', 'glass_table*9', '1', '2', '2', '1.00', '#ffffff,#E14218,#E14218,#ffffff', '0', '1', '1', '18', '218', 'Glass table', 'Translucent beauty', '4');
INSERT INTO `catalogue_items` VALUES ('219', 'glass_chair*9', '2', '1', '1', '1.00', '#ffffff,#E14218,#E14218,#ffffff', '0', '1', '1', '18', '219', 'Glass chair', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('220', 'glass_stool*9', '2', '1', '1', '1.00', '#ffffff,#E14218,#E14218,#ffffff', '0', '1', '1', '18', '220', 'Glass stool', 'Translucent beauty', '3');
INSERT INTO `catalogue_items` VALUES ('221', 'gothic_chair*3', '2', '1', '1', '1.00', '#ffffff,#dd0000,#ffffff,#dd0000', '0', '1', '1', '19', '221', 'Red Gothic Chair', 'The head of the table', '10');
INSERT INTO `catalogue_items` VALUES ('222', 'gothic_sofa*3', '2', '2', '1', '1.00', '#ffffff,#dd0000,#ffffff,#ffffff,#dd0000,#ffffff', '0', '1', '1', '19', '222', 'Red Gothic Sofa', 'Not great for hiding behind', '7');
INSERT INTO `catalogue_items` VALUES ('223', 'gothic_stool*3', '1', '1', '1', '1.00', '#ffffff,#dd0000,#ffffff', '0', '1', '1', '19', '223', 'Red Gothic Stool', 'Be calm. Be seated', '5');
INSERT INTO `catalogue_items` VALUES ('224', 'gothic_carpet', '4', '2', '4', '0.00', '0,0,0', '0', '1', '1', '19', '224', 'Cobbled Path', 'The route less travelled', '5');
INSERT INTO `catalogue_items` VALUES ('225', 'gothic_carpet2', '4', '2', '4', '0.00', '0,0,0', '0', '1', '1', '19', '225', 'Dungeon Floor', 'What lies beneath?', '5');
INSERT INTO `catalogue_items` VALUES ('226', 'goth_table', '1', '1', '5', '1.00', '0,0,0', '0', '1', '1', '19', '226', 'Gothic Table', 'A table with two heads', '15');
INSERT INTO `catalogue_items` VALUES ('227', 'gothrailing', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '19', '227', 'Gothic Railing', 'Keep out!', '8');
INSERT INTO `catalogue_items` VALUES ('228', 'torch', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '19', '228', 'Gothic Torch', 'Keeping dungeons light', '10');
INSERT INTO `catalogue_items` VALUES ('229', 'gothicfountain', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '19', '229', 'Gothic Fountain', 'The dark side of Habbo', '10');
INSERT INTO `catalogue_items` VALUES ('230', 'gothiccandelabra', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '19', '230', 'Gothic Candleabra', 'Through darkness came light', '10');
INSERT INTO `catalogue_items` VALUES ('231', 'gothgate', '1', '2', '1', '1.00', '0,0,0', '1', '1', '1', '19', '231', 'Gothic Portcullis', 'Don\'t get caught beneath!', '10');
INSERT INTO `catalogue_items` VALUES ('232', 'industrialfan', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '19', '232', 'Industrial Turbine', 'Powerful and resilient', '10');
INSERT INTO `catalogue_items` VALUES ('233', 'grunge_barrel', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '20', '233', 'Flaming Barrel', 'Beacon of light!', '3');
INSERT INTO `catalogue_items` VALUES ('234', 'grunge_bench', '2', '3', '1', '1.00', '0,0,0', '0', '1', '1', '20', '234', 'Bench', 'Laid back seating', '3');
INSERT INTO `catalogue_items` VALUES ('235', 'grunge_candle', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '20', '235', 'Candle Box', 'Late night debate', '2');
INSERT INTO `catalogue_items` VALUES ('236', 'grunge_chair', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '20', '236', 'Grunge Chair', 'Alternative chair for alternative people', '4');
INSERT INTO `catalogue_items` VALUES ('237', 'grunge_mattress', '2', '3', '1', '1.00', '0,0,0', '0', '1', '1', '20', '237', 'Grunge Mattress', 'Beats sleeping on the floor!', '3');
INSERT INTO `catalogue_items` VALUES ('238', 'grunge_radiator', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '20', '238', 'Radiator', 'Started cool but now it\'s hot!', '3');
INSERT INTO `catalogue_items` VALUES ('239', 'grunge_shelf', '1', '3', '1', '1.00', '0,0,0', '0', '1', '1', '20', '239', 'Grunge Bookshelf', 'Scrap books and photo albums', '5');
INSERT INTO `catalogue_items` VALUES ('240', 'grunge_sign', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '20', '240', 'Road Sign', 'Bought legitimately from an M1 cafe.', '3');
INSERT INTO `catalogue_items` VALUES ('241', 'grunge_table', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '20', '241', 'Grunge Table', 'Students of the round table!', '4');
INSERT INTO `catalogue_items` VALUES ('242', 'CF_1_coin_bronze', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '21', '242', 'Bronze Coin', 'Worth: 1 Habbo Credit', '1');
INSERT INTO `catalogue_items` VALUES ('243', 'CF_5_coin_silver', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '21', '243', 'Silver Coin', 'Worth: 5 Habbo Credits', '5');
INSERT INTO `catalogue_items` VALUES ('244', 'CF_10_coin_gold', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '21', '244', 'Gold Coin', 'Worth: 10 Habbo Credits', '10');
INSERT INTO `catalogue_items` VALUES ('245', 'CF_20_moneybag', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '21', '245', 'Money Bag', 'Worth: 20 Habbo Credits', '20');
INSERT INTO `catalogue_items` VALUES ('246', 'CF_50_goldbar', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '21', '246', 'Gold Bars', 'Worth: 50 Habbo Credits', '50');
INSERT INTO `catalogue_items` VALUES ('247', 'habbowood_chair', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '22', '247', 'Habbowood Chair', 'null', '5');
INSERT INTO `catalogue_items` VALUES ('248', 'rope_divider', '1', '2', '1', '0.00', '0,0,0', '1', '1', '1', '22', '248', 'Rope Divider', 'null', '5');
INSERT INTO `catalogue_items` VALUES ('249', 'spotlight', '1', '1', '1', '0.00', '0,0,0', '0', '1', '1', '22', '249', 'Spotlight', 'Light, camera, action!', '15');
INSERT INTO `catalogue_items` VALUES ('250', 'theatre_seat', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '22', '250', 'Theatre Seat', 'It\'s a Theatre Seat', '10');
INSERT INTO `catalogue_items` VALUES ('251', 'rare_icecream_campaign', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '22', '251', 'Icecream Machine', 'null', '25');
INSERT INTO `catalogue_items` VALUES ('252', 'habw_mirror', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '22', '252', 'Habbowood Mirror', 'Starts only!', '10');
INSERT INTO `catalogue_items` VALUES ('253', 'chair_norja', '2', '1', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '23', '253', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('254', 'couch_norja', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '23', '254', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('255', 'table_norja_med', '1', '2', '2', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '23', '255', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('256', 'shelves_norja', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '23', '256', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('257', 'soft_sofachair_norja', '2', '1', '1', '1.00', '#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '23', '257', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('258', 'soft_sofa_norja', '2', '2', '1', '1.00', '#ffffff,#F7EBBC,#ffffff,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '23', '258', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('259', 'divider_nor2', '1', '2', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '23', '259', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('260', 'divider_nor1', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '23', '260', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('261', 'divider_nor3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '1', '1', '1', '23', '261', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('262', 'divider_nor4', '1', '2', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC', '1', '1', '1', '23', '262', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('263', 'divider_nor5', '1', '1', '1', '1.00', '#ffffff,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC', '1', '1', '1', '23', '263', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('264', 'jp_bamboo', '4', '2', '2', '1.00', '0,0,0', '0', '1', '1', '24', '264', 'Bamboo Forest', 'Don\'t get lost', '25');
INSERT INTO `catalogue_items` VALUES ('265', 'jp_pillow', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '24', '265', 'Pillow', 'null', '5');
INSERT INTO `catalogue_items` VALUES ('266', 'jp_irori', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '24', '266', 'Sushi Bar', 'null', '10');
INSERT INTO `catalogue_items` VALUES ('267', 'jp_tatami', '4', '2', '2', '1.00', '0,0,0', '0', '1', '1', '24', '267', 'Tatami', 'null', '6');
INSERT INTO `catalogue_items` VALUES ('268', 'jp_tatami2', '4', '2', '4', '1.00', '0,0,0', '0', '1', '1', '24', '268', 'Tatami', 'null', '8');
INSERT INTO `catalogue_items` VALUES ('269', 'jp_lantern', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '24', '269', 'Lantern', 'null', '10');
INSERT INTO `catalogue_items` VALUES ('270', 'jp_drawer', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '24', '270', 'Drawers', 'null', '6');
INSERT INTO `catalogue_items` VALUES ('271', 'jp_ninjastars', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '24', '271', 'Ninjastars', 'Ouch!', '10');
INSERT INTO `catalogue_items` VALUES ('272', 'bed_armas_two', '2', '2', '3', '1.00', '0,0,0', '0', '1', '1', '25', '272', 'Double Bed', 'King-sized pine comfort', '3');
INSERT INTO `catalogue_items` VALUES ('273', 'bed_armas_one', '2', '1', '3', '1.00', '0,0,0', '0', '1', '1', '25', '273', 'Single Bed', 'Rustic charm for one', '3');
INSERT INTO `catalogue_items` VALUES ('274', 'fireplace_armas', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '25', '274', 'Fireplace', 'Authentic, real flame fire', '4');
INSERT INTO `catalogue_items` VALUES ('275', 'bartable_armas', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '25', '275', 'Bardesk', 'Bar-Style Table - essential for extra guests', '3');
INSERT INTO `catalogue_items` VALUES ('276', 'table_armas', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '25', '276', 'Dining Table', 'For informal dining', '3');
INSERT INTO `catalogue_items` VALUES ('277', 'bench_armas', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '25', '277', 'Bench', 'To complete the dining set', '3');
INSERT INTO `catalogue_items` VALUES ('278', 'divider_arm3', '1', '1', '1', '1.00', '0,0,0', '1', '1', '1', '25', '278', 'Gate (lockable)', 'Knock, knock...', '6');
INSERT INTO `catalogue_items` VALUES ('279', 'divider_arm1', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '279', 'Corner plinth', 'Good solid wood', '3');
INSERT INTO `catalogue_items` VALUES ('280', 'divider_arm2', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '25', '280', 'Room divider', 'I wooden go there', '3');
INSERT INTO `catalogue_items` VALUES ('281', 'shelves_armas', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '25', '281', 'Bookcase', 'For all those fire-side stories', '3');
INSERT INTO `catalogue_items` VALUES ('282', 'bar_armas', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '282', 'Barrel Minibar', 'It\'s a barrel of laughs and a great talking point', '4');
INSERT INTO `catalogue_items` VALUES ('283', 'bar_chair_armas', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '283', 'Barrel Stool', 'The ultimate recycled furniture', '1');
INSERT INTO `catalogue_items` VALUES ('284', 'lamp_armas', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '284', 'Table Lamp', 'Ambient lighting is essential', '3');
INSERT INTO `catalogue_items` VALUES ('285', 'lamp2_armas', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '285', 'Lodge Candle', 'Wax lyrical with some old-world charm', '3');
INSERT INTO `catalogue_items` VALUES ('286', 'small_table_armas', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '286', 'Occasional Table', 'Practical and beautiful', '2');
INSERT INTO `catalogue_items` VALUES ('287', 'small_chair_armas', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '25', '287', 'Stool', 'Rustic charm at it\'s best', '1');
INSERT INTO `catalogue_items` VALUES ('288', 'bed_polyfon', '2', '2', '3', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '26', '288', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('289', 'bed_polyfon_one', '2', '1', '3', '1.00', 'ffffff,#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '26', '289', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('290', 'fireplace_polyfon', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '26', '290', 'Fireplace', 'Comfort in stainless steel', '5');
INSERT INTO `catalogue_items` VALUES ('291', 'sofa_polyfon', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ABD0D2,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '26', '291', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('292', 'sofachair_polyfon', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '26', '292', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('293', 'bar_polyfon', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '26', '293', 'Mini-Bar', 'You naughty Habbo!', '5');
INSERT INTO `catalogue_items` VALUES ('294', 'divider_poly3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ABD0D2,#ABD0D2', '1', '1', '1', '26', '294', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('295', 'bardesk_polyfon', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '26', '295', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('296', 'bardeskcorner_polyfon', '1', '1', '1', '1.00', '#ffffff,#ABD0D2', '0', '1', '1', '26', '296', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('297', 'chair_polyfon', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '26', '297', 'Dining Chair', 'Metallic seating experience', '3');
INSERT INTO `catalogue_items` VALUES ('298', 'table_polyfon', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '26', '298', 'Large Coffee Table', 'For larger gatherings', '4');
INSERT INTO `catalogue_items` VALUES ('299', 'table_polyfon_med', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '26', '299', 'Large Coffee Table', 'For larger gatherings', '3');
INSERT INTO `catalogue_items` VALUES ('300', 'table_polyfon_small', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '26', '300', 'Small Coffee Table', 'For serving a stylish latte', '1');
INSERT INTO `catalogue_items` VALUES ('301', 'smooth_table_polyfon', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '26', '301', 'Large Dining Table', 'For larger gatherings', '4');
INSERT INTO `catalogue_items` VALUES ('302', 'stand_polyfon_z', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '26', '302', 'Shelf', 'Tidy up', '1');
INSERT INTO `catalogue_items` VALUES ('303', 'shelves_polyfon', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '26', '303', 'Bookcase', 'For the arty pad', '4');
INSERT INTO `catalogue_items` VALUES ('304', 'present_gen1', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('305', 'present_gen2', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('306', 'present_gen3', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('307', 'present_gen4', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('308', 'present_gen5', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('309', 'present_gen6', '1', '1', '1', '1.00', '0,0,0', '0', '0', '0', '-1', '0', '', '', '0');
INSERT INTO `catalogue_items` VALUES ('313', 'giftflowers', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '313', 'Vase of Flowers', 'Guaranteed to stay fresh', '4');
INSERT INTO `catalogue_items` VALUES ('314', 'plant_rose', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '314', 'Cut Roses', 'Sleek and chic', '3');
INSERT INTO `catalogue_items` VALUES ('315', 'plant_sunflower', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '315', 'Cut Sunflower', 'For happy Habbos', '3');
INSERT INTO `catalogue_items` VALUES ('316', 'plant_yukka', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '316', 'Yukka Plant', 'Easy to care for', '3');
INSERT INTO `catalogue_items` VALUES ('317', 'plant_pineapple', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '317', 'Pineapple Plant', 'Needs loving glances', '3');
INSERT INTO `catalogue_items` VALUES ('318', 'plant_bonsai', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '318', 'Bonsai Tree', 'You can be sure it lives', '3');
INSERT INTO `catalogue_items` VALUES ('319', 'plant_big_cactus', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '319', 'Mature Cactus', 'Habbo Dreams monster in hiding!  Shhhh', '3');
INSERT INTO `catalogue_items` VALUES ('320', 'plant_fruittree', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '320', 'Fruit Tree', 'Great yield and sweet fruit', '3');
INSERT INTO `catalogue_items` VALUES ('321', 'plant_small_cactus', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '321', 'Small Cactus', 'Even less watering than the real world', '1');
INSERT INTO `catalogue_items` VALUES ('322', 'plant_maze', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '28', '322', 'Maze shrubbery', 'Labyrinths Galore!', '5');
INSERT INTO `catalogue_items` VALUES ('323', 'plant_mazegate', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '28', '323', 'Maze Shrubbery Gate', 'Labyrinths Galore!', '6');
INSERT INTO `catalogue_items` VALUES ('324', 'plant_bulrush', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '28', '324', 'Bulrush', 'Ideal for the riverside', '3');
INSERT INTO `catalogue_items` VALUES ('325', 'chair_plasto*16', '2', '1', '1', '1.00', '#ffffff,#CC3399,#ffffff,#CC3399', '0', '1', '1', '29', '325', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('326', 'chair_plasto*15', '2', '1', '1', '1.00', '#ffffff,#FF97BA,#ffffff,#FF97BA', '0', '1', '1', '29', '326', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('327', 'chair_plasto*5', '2', '1', '1', '1.00', '#ffffff,#54ca00,#ffffff,#54ca00', '0', '1', '1', '29', '327', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('328', 'chair_plasto', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '29', '328', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('329', 'chair_plasto*8', '2', '1', '1', '1.00', '#ffffff,#c38d1a,#ffffff,#c38d1a', '0', '1', '1', '29', '329', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('330', 'chair_plasto*7', '2', '1', '1', '1.00', '#ffffff,#ff6d00,#ffffff,#ff6d00', '0', '1', '1', '29', '330', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('331', 'chair_plasto*1', '2', '1', '1', '1.00', '#ffffff,#ff1f08,#ffffff,#ff1f08', '0', '1', '1', '29', '331', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('332', 'chair_plasto*4', '2', '1', '1', '1.00', '#ffffff,#ccddff,#ffffff,#ccddff', '0', '1', '1', '29', '332', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('333', 'chair_plasto*6', '2', '1', '1', '1.00', '#ffffff,#3444ff,#ffffff,#3444ff', '0', '1', '1', '29', '333', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('334', 'chair_plasto*3', '2', '1', '1', '1.00', '#ffffff,#ffee00,#ffffff,#ffee00', '0', '1', '1', '29', '334', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('335', 'chair_plasto*2', '2', '1', '1', '1.00', '#ffffff,#99DCCC,#ffffff,#99DCCc', '0', '1', '1', '29', '335', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('336', 'table_plasto_square*15', '1', '1', '1', '1.00', '#ffffff,#FF97BA', '0', '1', '1', '29', '336', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('337', 'table_plasto_square*14', '1', '1', '1', '1.00', '#ffffff,#CC3399', '0', '1', '1', '29', '337', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('338', 'table_plasto_square*1', '1', '1', '1', '1.00', '#ffffff,#ff1f08', '0', '1', '1', '29', '338', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('339', 'table_plasto_square*7', '1', '1', '1', '1.00', '#ffffff,#ff6d00', '0', '1', '1', '29', '339', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('340', 'table_plasto_square', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '29', '340', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('341', 'table_plasto_square*2', '1', '1', '1', '1.00', '#ffffff,#99DCCC', '0', '1', '1', '29', '341', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('342', 'table_plasto_square*4', '1', '1', '1', '1.00', '#ffffff,#ccddff', '0', '1', '1', '29', '342', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('343', 'table_plasto_square*6', '1', '1', '1', '1.00', '#ffffff,#3444ff', '0', '1', '1', '29', '343', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('344', 'table_plasto_square*3', '1', '1', '1', '1.00', '#ffffff,#ffee00', '0', '1', '1', '29', '344', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('345', 'table_plasto_square*9', '1', '1', '1', '1.00', '#ffffff,#533e10', '0', '1', '1', '29', '345', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('346', 'table_plasto_square*5', '1', '1', '1', '1.00', '#ffffff,#54ca00', '0', '1', '1', '29', '346', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('347', 'table_plasto_square*8', '1', '1', '1', '1.00', '#ffffff,#c38d1a', '0', '1', '1', '29', '347', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('348', 'table_plasto_round*15', '1', '2', '2', '1.00', '#ffffff,#FF97BA', '0', '1', '1', '29', '348', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('349', 'table_plasto_round*14', '1', '2', '2', '1.00', '#ffffff,#CC3399', '0', '1', '1', '29', '349', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('350', 'table_plasto_round*7', '1', '2', '2', '1.00', '#ffffff,#ff6d00', '0', '1', '1', '29', '350', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('351', 'table_plasto_round*1', '1', '2', '2', '1.00', '#ffffff,#ff1f08', '0', '1', '1', '29', '351', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('352', 'table_plasto_round*2', '1', '2', '2', '1.00', '#ffffff,#99DCCC', '0', '1', '1', '29', '352', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('353', 'table_plasto_round*4', '1', '2', '2', '1.00', '#ffffff,#ccddff', '0', '1', '1', '29', '353', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('354', 'table_plasto_round*6', '1', '2', '2', '1.00', '#ffffff,#3444ff', '0', '1', '1', '29', '354', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('355', 'table_plasto_round*3', '1', '2', '2', '1.00', '#ffffff,#ffee00', '0', '1', '1', '29', '355', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('356', 'table_plasto_round*9', '1', '2', '2', '1.00', '#ffffff,#533e10', '0', '1', '1', '29', '356', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('357', 'table_plasto_round', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '29', '357', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('358', 'table_plasto_round*5', '1', '2', '2', '1.00', '#ffffff,#54ca00', '0', '1', '1', '29', '358', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('359', 'table_plasto_round*8', '1', '2', '2', '1.00', '#ffffff,#c38d1a', '0', '1', '1', '29', '359', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('360', 'table_plasto_bigsquare*15', '1', '2', '2', '1.00', '#ffffff,#FF97BA', '0', '1', '1', '29', '360', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('361', 'table_plasto_bigsquare*14', '1', '2', '2', '1.00', '#ffffff,#CC3399', '0', '1', '1', '29', '361', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('362', 'table_plasto_bigsquare*7', '1', '2', '2', '1.00', '#ffffff,#ff6d00', '0', '1', '1', '29', '362', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('363', 'table_plasto_bigsquare*1', '1', '2', '2', '1.00', '#ffffff,#ff1f08', '0', '1', '1', '29', '363', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('364', 'table_plasto_bigsquare*2', '1', '2', '2', '1.00', '#ffffff,#99DCCC', '0', '1', '1', '29', '364', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('365', 'table_plasto_bigsquare', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '29', '365', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('366', 'table_plasto_bigsquare*8', '1', '2', '2', '1.00', '#ffffff,#c38d1a', '0', '1', '1', '29', '366', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('367', 'table_plasto_bigsquare*5', '1', '2', '2', '1.00', '#ffffff,#54ca00', '0', '1', '1', '29', '367', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('368', 'table_plasto_bigsquare*9', '1', '2', '2', '1.00', '#ffffff,#533e10', '0', '1', '1', '29', '368', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('369', 'table_plasto_bigsquare*3', '1', '2', '2', '1.00', '#ffffff,#ffee00', '0', '1', '1', '29', '369', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('370', 'table_plasto_bigsquare*6', '1', '2', '2', '1.00', '#ffffff,#3444ff', '0', '1', '1', '29', '370', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('371', 'table_plasto_bigsquare*4', '1', '2', '2', '1.00', '#ffffff,#ccddff', '0', '1', '1', '29', '371', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('372', 'table_plasto_4leg*6', '1', '2', '2', '1.00', '#ffffff,#3444ff', '0', '1', '1', '29', '372', 'Occasional table Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('373', 'table_plasto_4leg*1', '1', '2', '2', '1.00', '#ffffff,#ff1f08', '0', '1', '1', '29', '373', 'Square Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('374', 'table_plasto_4leg*3', '1', '2', '2', '1.00', '#ffffff,#ffee00', '0', '1', '1', '29', '374', 'Round Dining Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('375', 'table_plasto_4leg*9', '1', '2', '2', '1.00', '#ffffff,#533e10', '0', '1', '1', '29', '375', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('376', 'table_plasto_4leg', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '29', '376', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('377', 'table_plasto_4leg*5', '1', '2', '2', '1.00', '#ffffff,#54ca00', '0', '1', '1', '29', '377', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('378', 'table_plasto_4leg*2', '1', '2', '2', '1.00', '#ffffff,#99DCCC', '0', '1', '1', '29', '378', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('379', 'table_plasto_4leg*8', '1', '2', '2', '1.00', '#ffffff,#c38d1a', '0', '1', '1', '29', '379', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('380', 'table_plasto_4leg*7', '1', '2', '2', '1.00', '#ffffff,#ff6d00', '0', '1', '1', '29', '380', 'Occasional table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('381', 'table_plasto_4leg*10', '1', '2', '2', '1.00', '#ffffff,#ccddff', '0', '1', '1', '29', '381', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('382', 'table_plasto_4leg*15', '1', '2', '2', '1.00', '#ffffff,#FF97BA', '0', '1', '1', '29', '382', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('383', 'table_plasto_4leg*16', '1', '2', '2', '1.00', '#ffffff,#CC3399', '0', '1', '1', '29', '383', 'Occasional Table', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('384', 'chair_plasty', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '29', '384', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('385', 'chair_plasty*1', '2', '1', '1', '1.00', '#ffffff,#8EB5D1,#ffffff,#8EB5D1', '0', '1', '1', '29', '385', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('386', 'chair_plasty*2', '2', '1', '1', '1.00', '#ffffff,#ff9900,#ffffff,#ff9900', '0', '1', '1', '29', '386', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('387', 'chair_plasty*3', '2', '1', '1', '1.00', '#ffffff,#ff2200,#ffffff,#ff2200', '0', '1', '1', '29', '387', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('388', 'chair_plasty*4', '2', '1', '1', '1.00', '#ffffff,#00cc00,#ffffff,#00cc00', '0', '1', '1', '29', '388', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('389', 'chair_plasty*4', '2', '1', '1', '1.00', '#ffffff,#0033CC,#ffffff,#0033CC', '0', '1', '1', '29', '389', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('390', 'chair_plasty*4', '2', '1', '1', '1.00', '#ffffff,#333333,#ffffff,#333333', '0', '1', '1', '29', '390', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('391', 'chair_plasty*7', '2', '1', '1', '1.00', '#ffffff,#99DCCc,#ffffff,#99DCCc', '0', '1', '1', '29', '391', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('392', 'chair_plasty*8', '2', '1', '1', '1.00', '#ffffff,#c38d1a,#ffffff,#c38d1a', '0', '1', '1', '29', '392', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('393', 'chair_plasty*9', '2', '1', '1', '1.00', '#ffffff,#533e10,#ffffff,#533e10', '0', '1', '1', '29', '393', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('394', 'chair_plasty*10', '2', '1', '1', '1.00', '#ffffff,#CC3399,#ffffff,#CC3399', '0', '1', '1', '29', '394', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('395', 'chair_plasty*11', '2', '1', '1', '1.00', '#ffffff,#FF97BA,#ffffff,#FF97BA', '0', '1', '1', '29', '395', 'Plastic Pod Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('396', 'chair_plasto*9', '2', '1', '1', '1.00', '#ffffff,#533e10,#ffffff,#533e10', '0', '1', '1', '29', '396', 'Chair', 'Hip plastic furniture', '3');
INSERT INTO `catalogue_items` VALUES ('397', 'shelves_basic', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '31', '397', 'Pura Shelves', 'Pura Series 404 shelves', '3');
INSERT INTO `catalogue_items` VALUES ('398', 'bar_basic', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '31', '398', 'A Pura Minibar', 'A Pura series 300 minibar', '4');
INSERT INTO `catalogue_items` VALUES ('399', 'fridge', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '31', '399', 'Pura Refridgerator', 'Keep cool with a chilled snack or drink', '6');
INSERT INTO `catalogue_items` VALUES ('400', 'lamp_basic', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '31', '400', 'Pura Lamp', 'Switch on the atmosphere with this sophisticated light', '3');
INSERT INTO `catalogue_items` VALUES ('401', 'bed_budgetb', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '31', '401', 'White Plain Double Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('402', 'bed_budgetb_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '31', '402', 'White Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('403', 'bed_budget', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '31', '403', 'Aqua Plain Double Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('404', 'bed_budget_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '31', '404', 'Aqua Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('405', 'bed_budgetb', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FF99BC,#FF99BC,#FF99BC', '0', '1', '1', '31', '405', 'Pink Plain Double Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('406', 'bed_budgetb_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FF99BC,#FF99BC,#FF99BC', '0', '1', '1', '31', '406', 'Pink Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('407', 'bed_budget', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#525252,#525252,#525252', '0', '1', '1', '31', '407', 'Black Plain Double Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('408', 'bed_budget_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#525252,#525252,#525252', '0', '1', '1', '31', '408', 'Black Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('409', 'bed_budgetb', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '31', '409', 'Beige Plain Double Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('410', 'bed_budgetb_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '31', '410', 'Beige Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('411', 'bed_budget', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '31', '411', 'Blue Plain Single Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('412', 'bed_budget_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '31', '412', 'Blue Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('413', 'bed_budgetb', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#92D13D,#92D13D,#92D13D', '0', '1', '1', '31', '413', 'Green Plain Single Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('414', 'bed_budgetb_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#92D13D,#92D13D,#92D13D', '0', '1', '1', '31', '414', 'Green Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('415', 'bed_budget', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFD837,#FFD837,#FFD837', '0', '1', '1', '31', '415', 'Yellow Plain Single Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('416', 'bed_budget_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFD837,#FFD837,#FFD837', '0', '1', '1', '31', '416', 'Yellow Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('417', 'bed_budgetb', '2', '2', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#E14218,#E14218,#E14218', '0', '1', '1', '31', '417', 'Red Plain Single Bed', 'Sweet dreams for two', '3');
INSERT INTO `catalogue_items` VALUES ('418', 'bed_budgetb_one', '2', '1', '3', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#E14218,#E14218,#E14218', '0', '1', '1', '31', '418', 'Red Plain Single Bed', 'All you need for a good night\'s kip', '3');
INSERT INTO `catalogue_items` VALUES ('419', 'rclr_sofa', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '-1', '419', 'Recycler Sofa', 'Handmade...', '25');
INSERT INTO `catalogue_items` VALUES ('420', 'rclr_chair', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '-1', '420', 'Recycler Chair', 'Handmade...', '25');
INSERT INTO `catalogue_items` VALUES ('421', 'rclr_garden', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '-1', '421', 'Recycler Garden', 'Give them water!', '25');
INSERT INTO `catalogue_items` VALUES ('422', 'queue_tile1*0', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '33', '422', 'White Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('423', 'queue_tile1*1', '1', '1', '1', '1.00', '#ffffff,#FF7C98,#ffffff,#ffffff', '0', '1', '1', '33', '423', 'Pink Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('424', 'queue_tile1*2', '1', '1', '1', '1.00', '#ffffff,#FF3333,#ffffff,#ffffff', '0', '1', '1', '33', '424', 'Red Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('425', 'queue_tile1*3', '1', '1', '1', '1.00', '#ffffff,#67c39c,#ffffff,#ffffff', '0', '1', '1', '33', '425', 'Aqua Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('426', 'queue_tile1*4', '1', '1', '1', '1.00', '#ffffff,#FFAA2B,#ffffff,#ffffff', '0', '1', '1', '33', '426', 'Gold Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('427', 'queue_tile1*5', '1', '1', '1', '1.00', '#ffffff,#555A37,#ffffff,#ffffff', '0', '1', '1', '33', '427', 'Knight Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('428', 'queue_tile1*6', '1', '1', '1', '1.00', '#ffffff,#A2E8FA,#ffffff,#ffffff', '0', '1', '1', '33', '428', 'Blue Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('429', 'queue_tile1*7', '1', '1', '1', '1.00', '#ffffff,#FC5AFF,#ffffff,#ffffff', '0', '1', '1', '33', '429', 'Purple Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('430', 'queue_tile1*8', '1', '1', '1', '1.00', '#ffffff,#1E8AA5,#ffffff,#ffffff', '0', '1', '1', '33', '430', 'Navy Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('431', 'queue_tile1*9', '1', '1', '1', '1.00', '#ffffff,#9AFF60,#ffffff,#ffffff', '0', '1', '1', '33', '431', 'Green Habbo Roller', 'The power of movement', '5');
INSERT INTO `catalogue_items` VALUES ('432', 'grand_piano*1', '1', '2', '2', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '34', '432', 'Rose Quartz Grand Piano', 'Chopin\'s revolutionary instrument', '10');
INSERT INTO `catalogue_items` VALUES ('433', 'romantique_pianochair*1', '2', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '34', '433', 'Rose Quartz Piano Stool', 'Here sat the legend of 1900', '2');
INSERT INTO `catalogue_items` VALUES ('434', 'romantique_divan*1', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FF8B8B', '0', '1', '1', '34', '434', 'Rose Quartz Chaise-Longue', 'Recline in continental comfort', '6');
INSERT INTO `catalogue_items` VALUES ('435', 'romantique_chair*1', '2', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '34', '435', 'Rose Quartz Chair', 'Elegant seating for elegant Habbos', '5');
INSERT INTO `catalogue_items` VALUES ('436', 'romantique_divider*1', '1', '2', '1', '1.00', '#FF8B8B,#FF8B8B,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '34', '436', 'Rose Quartz Screen', 'Beauty lies within', '6');
INSERT INTO `catalogue_items` VALUES ('437', 'romantique_smalltabl*1', '1', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '34', '437', 'Rose Quartz Tray Table', 'Every tray needs a table...', '4');
INSERT INTO `catalogue_items` VALUES ('438', 'wallmirror', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '34', '438', 'Wall Mirror', 'Mirror, mirror on the wall', '4');
INSERT INTO `catalogue_items` VALUES ('439', 'romantique_tray2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '34', '439', 'Treats Tray', 'Spoil yourself', '5');
INSERT INTO `catalogue_items` VALUES ('440', 'rom_lamp', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '34', '440', 'Crystal Lamp', 'Light up your life', '6');
INSERT INTO `catalogue_items` VALUES ('441', 'romantique_mirrortabl', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '34', '441', 'Dressing Table', 'Get ready for your big date', '8');
INSERT INTO `catalogue_items` VALUES ('442', 'carpet_standard', '4', '3', '5', '0.00', '0,0,0', '0', '1', '1', '35', '442', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('443', 'carpet_standard*a', '4', '3', '5', '0.00', '#55AC00,#55AC00,#55AC00', '0', '1', '1', '35', '443', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('444', 'carpet_standard*b', '4', '3', '5', '0.00', '#336666,#336666,#336666', '0', '1', '1', '35', '444', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('445', 'carpet_standard*1', '4', '3', '5', '0.00', '#ff1f08', '0', '1', '1', '35', '445', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('446', 'carpet_standard*2', '4', '3', '5', '0.00', '#99DCCC', '0', '1', '1', '35', '446', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('447', 'carpet_standard*3', '4', '3', '5', '0.00', '#ffee00', '0', '1', '1', '35', '447', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('448', 'carpet_standard*4', '4', '3', '5', '0.00', '#ccddff', '0', '1', '1', '35', '448', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('449', 'carpet_standard*5', '4', '3', '5', '0.00', '#ddccff', '0', '1', '1', '35', '449', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('450', 'carpet_standard*6', '4', '3', '5', '0.00', '#777777', '0', '1', '1', '35', '450', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('451', 'carpet_standard*7', '4', '3', '5', '0.00', '#99CCFF,#99CCFF,#99CCFF', '0', '1', '1', '35', '451', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('452', 'carpet_standard*8', '4', '3', '5', '0.00', '#999966,#999966,#999966', '0', '1', '1', '35', '452', 'Floor Rug', 'Available in a variety of colours', '3');
INSERT INTO `catalogue_items` VALUES ('453', 'carpet_soft', '4', '2', '4', '0.00', '', '0', '1', '1', '35', '453', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('454', 'carpet_soft*1', '4', '2', '4', '0.00', '#CC3333', '0', '1', '1', '35', '454', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('455', 'carpet_soft*2', '4', '2', '4', '0.00', '#66FFFF', '0', '1', '1', '35', '455', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('456', 'carpet_soft*3', '4', '2', '4', '0.00', '#FFCC99', '0', '1', '1', '35', '456', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('457', 'carpet_soft*4', '4', '2', '4', '0.00', '#FFCCFF', '0', '1', '1', '35', '457', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('458', 'carpet_soft*5', '4', '2', '4', '0.00', '#FFFF66', '0', '1', '1', '35', '458', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('459', 'carpet_soft*6', '4', '2', '4', '0.00', '#669933', '0', '1', '1', '35', '459', 'Soft Wool Rug', 'Soft Wool Rug', '3');
INSERT INTO `catalogue_items` VALUES ('460', 'doormat_love', '4', '1', '1', '0.00', '0,0,0', '0', '1', '1', '35', '460', 'Doormat', 'Welcome Habbos in style', '1');
INSERT INTO `catalogue_items` VALUES ('461', 'doormat_plain', '4', '1', '1', '0.00', '0,0,0', '0', '1', '1', '35', '461', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('462', 'doormat_plain*1', '4', '1', '1', '0.00', '#ff1f08', '0', '1', '1', '35', '462', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('463', 'doormat_plain*2', '4', '1', '1', '0.00', '#99DCCC', '0', '1', '1', '35', '463', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('464', 'doormat_plain*3', '4', '1', '1', '0.00', '#ffee00', '0', '1', '1', '35', '464', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('465', 'doormat_plain*4', '4', '1', '1', '0.00', '#ccddff', '0', '1', '1', '35', '465', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('466', 'doormat_plain*5', '4', '1', '1', '0.00', '#ddccff', '0', '1', '1', '35', '466', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('467', 'doormat_plain*6', '4', '1', '1', '0.00', '#777777', '0', '1', '1', '35', '467', 'Doormat', 'Available in a variety of colours', '1');
INSERT INTO `catalogue_items` VALUES ('468', 'carpet_armas', '4', '2', '4', '0.00', '0,0,0', '0', '1', '1', '35', '468', 'Hand-Woven Rug', 'Adds instant warmth', '3');
INSERT INTO `catalogue_items` VALUES ('469', 'carpet_polar', '4', '2', '3', '0.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '35', '469', 'Faux-Fur Bear Rug', 'For cuddling up on', '4');
INSERT INTO `catalogue_items` VALUES ('470', 'carpet_polar*2', '4', '2', '3', '0.00', '#ccddff,#ccddff,#ffffff', '0', '1', '1', '35', '470', 'Blue Bear Rug', 'Snuggle up on a Funky bear rug...', '10');
INSERT INTO `catalogue_items` VALUES ('471', 'carpet_polar*3', '4', '2', '3', '0.00', '#ffee99,#ffee99,#ffffff', '0', '1', '1', '35', '471', 'Yellow Bear Rug', 'Snuggle up on a Funky bear rug...', '10');
INSERT INTO `catalogue_items` VALUES ('472', 'carpet_polar*4', '4', '2', '3', '0.00', '#ddffaa,#ddffaa,#ffffff', '0', '1', '1', '35', '472', 'Green Bear Rug', 'Snuggle up on a Funky bear rug...', '10');
INSERT INTO `catalogue_items` VALUES ('473', 'poster 52', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '36', '473', 'Hockey stick', 'whack that ball!', '3');
INSERT INTO `catalogue_items` VALUES ('474', 'poster 53', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '36', '474', 'Hockey stick', 'whack that ball!', '3');
INSERT INTO `catalogue_items` VALUES ('475', 'poster 54', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '36', '475', 'Hockey stick', 'whack that ball!', '3');
INSERT INTO `catalogue_items` VALUES ('476', 'hockey_score', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '36', '476', 'Scoreboard', '...for keeping your score', '6');
INSERT INTO `catalogue_items` VALUES ('477', 'legotrophy', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '36', '477', 'Basketball Trophy', 'For the winning team', '3');
INSERT INTO `catalogue_items` VALUES ('478', 'poster 51', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '36', '478', 'Basketball Hoop', '2 points for every basket', '3');
INSERT INTO `catalogue_items` VALUES ('479', 'carpet_legocourt', '4', '3', '3', '1.00', '0,0,0', '0', '1', '1', '36', '479', 'Basketball Court', 'Line up your slam dunk', '3');
INSERT INTO `catalogue_items` VALUES ('480', 'bench_lego', '2', '4', '1', '1.00', '0,0,0', '0', '1', '1', '36', '480', 'Team Bench', 'For your reserve players', '6');
INSERT INTO `catalogue_items` VALUES ('481', 'sporttrack1*1', '4', '2', '2', '1.00', '#ffffff,#e4592d,#ffffff', '0', '1', '1', '36', '481', 'Sport track straight', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('482', 'sporttrack1*2', '4', '2', '2', '1.00', '#ffffff,#62818b,#ffffff', '0', '1', '1', '36', '482', 'Sport track straight asphalt', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('483', 'sporttrack1*3', '4', '2', '2', '1.00', '#ffffff,#5cb800,#ffffff', '0', '1', '1', '36', '483', 'Sport track straight grass', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('484', 'sporttrack2*1', '4', '2', '2', '1.00', '#ffffff,#e4592d,#ffffff', '0', '1', '1', '36', '484', 'Sport corner', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('485', 'sporttrack2*2', '4', '2', '2', '1.00', '#ffffff,#62818b,#ffffff', '0', '1', '1', '36', '485', 'Sport corner asphalt', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('486', 'sporttrack2*3', '4', '2', '2', '1.00', '#ffffff,#5cb800,#ffffff', '0', '1', '1', '36', '486', 'Sport corner grass', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('488', 'sporttrack3*1', '4', '2', '2', '1.00', '#ffffff,#e4592d,#ffffff', '0', '1', '1', '36', '488', 'Sport goal', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('487', 'sporttrack3*3', '4', '2', '2', '1.00', '#ffffff,#5cb800,#ffffff', '0', '1', '1', '36', '487', 'Sport goal grass', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('489', 'sporttrack3*2', '4', '2', '2', '1.00', '#ffffff,#62818b,#ffffff', '0', '1', '1', '36', '489', 'Sport goal asphalt', 'Let\'s get sporty!', '3');
INSERT INTO `catalogue_items` VALUES ('490', 'footylamp', '4', '2', '2', '1.00', '0,0,0', '0', '1', '1', '36', '490', 'Football Lamp', 'Let\'s get sporty!', '10');
INSERT INTO `catalogue_items` VALUES ('491', 'door', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '37', '491', 'Telephone Box', 'Dr Who?', '5');
INSERT INTO `catalogue_items` VALUES ('492', 'doorC', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '37', '492', 'Portaloo', 'In a hurry?', '4');
INSERT INTO `catalogue_items` VALUES ('493', 'doorB', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '37', '493', 'Wardrobe', 'Narnia this way!', '3');
INSERT INTO `catalogue_items` VALUES ('494', 'teleport_door', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '37', '494', 'Teleport Door', 'Teleport Door', '6');
INSERT INTO `catalogue_items` VALUES ('495', 'sound_machine', '1', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#828282,#FFFFFF', '0', '1', '1', '38', '495', 'Sound Machine', 'Creating fancy sounds', '8');
INSERT INTO `catalogue_items` VALUES ('496', 'sound_set_4', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '496', 'Ambient 1', 'Chilled out beats', '5');
INSERT INTO `catalogue_items` VALUES ('497', 'sound_set_8', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '497', 'Ambient 2', 'Mellow electric grooves', '5');
INSERT INTO `catalogue_items` VALUES ('498', 'sound_set_6', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '498', 'Ambient 3', 'Background ambience loops', '5');
INSERT INTO `catalogue_items` VALUES ('499', 'sound_set_5', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '499', 'Ambient 4', 'The dark side of Habbo', '5');
INSERT INTO `catalogue_items` VALUES ('500', 'sound_set_26', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '500', 'Groove 1', 'Bollywood Beats!', '5');
INSERT INTO `catalogue_items` VALUES ('501', 'sound_set_27', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '501', 'Groove 2', 'Jingle Bells will never be the same...', '5');
INSERT INTO `catalogue_items` VALUES ('502', 'sound_set_17', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '502', 'Groove 3', 'Jive\'s Alive!', '5');
INSERT INTO `catalogue_items` VALUES ('503', 'sound_set_18', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '503', 'Groove 4', 'Listen while you tan', '5');
INSERT INTO `catalogue_items` VALUES ('504', 'sound_set_3', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '504', 'Electronic 1', 'Chilled grooves', '5');
INSERT INTO `catalogue_items` VALUES ('505', 'sound_set_9', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '38', '505', 'Electronic 2', 'Mystical ambient soundscapes', '5');
INSERT INTO `catalogue_items` VALUES ('506', 'jukebox*1', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '506', 'Jukebox', 'For your Happy Days!', '5');
INSERT INTO `catalogue_items` VALUES ('507', 'sound_set_46', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '507', 'Club 1', 'De bada bada bo!', '5');
INSERT INTO `catalogue_items` VALUES ('508', 'sound_set_47', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '508', 'Club 2', 'Storm the UKCharts!', '5');
INSERT INTO `catalogue_items` VALUES ('509', 'sound_set_48', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '509', 'Club 3', 'Sweet party beat', '5');
INSERT INTO `catalogue_items` VALUES ('510', 'sound_set_49', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '510', 'Club 4', 'You will belong', '5');
INSERT INTO `catalogue_items` VALUES ('511', 'sound_set_50', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '511', 'Club 5', 'The harder generation', '5');
INSERT INTO `catalogue_items` VALUES ('512', 'sound_set_51', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '512', 'Club 6', 'Bop to the beat', '5');
INSERT INTO `catalogue_items` VALUES ('513', 'sound_set_25', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '513', 'Dance 1', 'Actually, it\'s Partay!', '5');
INSERT INTO `catalogue_items` VALUES ('514', 'sound_set_29', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '514', 'Dance 2', 'Electronic house', '5');
INSERT INTO `catalogue_items` VALUES ('515', 'sound_set_31', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '515', 'Dance 3', 'House loops', '5');
INSERT INTO `catalogue_items` VALUES ('516', 'sound_set_11', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '516', 'Dance 4', 'Music you can really sink your teeth into', '5');
INSERT INTO `catalogue_items` VALUES ('517', 'sound_set_13', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '39', '517', 'Dance 5', 'Let Music be the food of Habbo', '5');
INSERT INTO `catalogue_items` VALUES ('518', 'sound_set_35', '1', '0', '1', '1.00', '0,0,0', '0', '1', '1', '39', '518', 'Dance 6', 'Groovelicious', '5');
INSERT INTO `catalogue_items` VALUES ('519', 'jukebox*1', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '519', 'Jukebox', 'For your Happy Days!', '5');
INSERT INTO `catalogue_items` VALUES ('520', 'guitar_v', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '520', 'V-Guitar', 'V-Guitar', '5');
INSERT INTO `catalogue_items` VALUES ('521', 'guitar_skull', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '521', 'Skull-Guitar', 'Skull-Guitar', '5');
INSERT INTO `catalogue_items` VALUES ('522', 'sound_set_21', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '522', 'Rock 1', 'Headbanging riffs', '5');
INSERT INTO `catalogue_items` VALUES ('523', 'sound_set_28', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '523', 'Rock 2', 'Head for the pit!', '5');
INSERT INTO `catalogue_items` VALUES ('524', 'sound_set_33', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '524', 'Rock 3', 'Guitar solo set', '5');
INSERT INTO `catalogue_items` VALUES ('525', 'sound_set_40', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '525', 'Rock 4', 'Dude? Cheese!', '5');
INSERT INTO `catalogue_items` VALUES ('526', 'sound_set_34', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '526', 'Rock 5', 'For guitar heroes', '5');
INSERT INTO `catalogue_items` VALUES ('527', 'sound_set_38', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '527', 'Rock 6', 'Rock and Roses!', '5');
INSERT INTO `catalogue_items` VALUES ('528', 'sound_set_39', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '528', 'Rock 7', 'Rock with a roll', '5');
INSERT INTO `catalogue_items` VALUES ('529', 'sound_set_41', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '40', '529', 'Rock 8', 'Burning Riffs', '5');
INSERT INTO `catalogue_items` VALUES ('530', 'jukebox*1', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '530', 'Jukebox', 'For your Happy Days!', '5');
INSERT INTO `catalogue_items` VALUES ('531', 'sound_set_1', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '531', 'Habbo Sounds 1', 'Get the party started!', '5');
INSERT INTO `catalogue_items` VALUES ('532', 'sound_set_12', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '532', 'Habbo Sounds 2', 'Unusual as Standard', '5');
INSERT INTO `catalogue_items` VALUES ('533', 'sound_set_2', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '533', 'Habbo Sounds 3', 'Get the party started!', '5');
INSERT INTO `catalogue_items` VALUES ('534', 'sound_set_24', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '534', 'Habbo Sounds 4', 'It\'s all about the Pentiums, baby!', '5');
INSERT INTO `catalogue_items` VALUES ('535', 'sound_set_43', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '535', 'SFX 1', 'Beware zombies!', '5');
INSERT INTO `catalogue_items` VALUES ('536', 'sound_set_20', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '536', 'SFX 2', 'Musical heaven', '5');
INSERT INTO `catalogue_items` VALUES ('537', 'sound_set_22', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '537', 'SFX 3', 'With a hamper full of sounds, not sarnies', '5');
INSERT INTO `catalogue_items` VALUES ('538', 'sound_set_23', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '538', 'SFX 4', 'Don\'t be afraid of the dark', '5');
INSERT INTO `catalogue_items` VALUES ('539', 'sound_set_7', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '539', 'SFX 5', 'Sound effects for Furni', '5');
INSERT INTO `catalogue_items` VALUES ('540', 'sound_set_30', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '540', 'Instrumental 1', 'Moments in love', '5');
INSERT INTO `catalogue_items` VALUES ('541', 'sound_set_32', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '41', '541', 'Instrumental 2', 'Piano concert set', '5');
INSERT INTO `catalogue_items` VALUES ('542', 'sound_set_36', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '542', 'Latin Love 1', 'For adult minded', '5');
INSERT INTO `catalogue_items` VALUES ('543', 'sound_set_60', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '543', 'Latin Love 2', 'Love and affection!', '5');
INSERT INTO `catalogue_items` VALUES ('544', 'sound_set_61', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '544', 'Latin Love 3', 'Straight from the heart', '5');
INSERT INTO `catalogue_items` VALUES ('545', 'sound_set_55', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '545', 'RnB Grooves 1', 'Can you fill me in?', '5');
INSERT INTO `catalogue_items` VALUES ('546', 'sound_set_56', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '546', 'RnB Grooves 2', 'Get down tonight!', '5');
INSERT INTO `catalogue_items` VALUES ('547', 'sound_set_57', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '547', 'RnB Grooves 3', 'Feel the groove', '5');
INSERT INTO `catalogue_items` VALUES ('548', 'sound_set_58', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '548', 'RnB Grooves 4', 'Sh-shake it!', '5');
INSERT INTO `catalogue_items` VALUES ('549', 'sound_set_59', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '549', 'RnB Grooves 5', 'Urban break beats', '5');
INSERT INTO `catalogue_items` VALUES ('550', 'sound_set_15', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '550', 'RnB Grooves 6', 'Unadulterated essentials', '5');
INSERT INTO `catalogue_items` VALUES ('551', 'sound_set_10', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '551', 'Hip Hop Beats 1', 'Made from real Boy Bands!', '5');
INSERT INTO `catalogue_items` VALUES ('552', 'sound_set_14', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '552', 'Hip Hop Beats 2', 'Rock them bodies', '5');
INSERT INTO `catalogue_items` VALUES ('553', 'sound_set_16', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '553', 'Hip Hop Beats 3', 'Ferry, ferry good!', '5');
INSERT INTO `catalogue_items` VALUES ('554', 'sound_set_19', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '42', '554', 'Hip Hop Beats 4', 'Shake your body!', '5');
INSERT INTO `catalogue_items` VALUES ('555', 'prizetrophy6*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '555', 'Champion trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('556', 'prizetrophy6*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '556', 'Champion trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('557', 'prizetrophy6*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '557', 'Champion trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('558', 'prizetrophy3*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '558', 'Globe trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('559', 'prizetrophy3*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '559', 'Globe trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('560', 'prizetrophy3*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '560', 'Globe trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('561', 'prizetrophy4*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '561', 'Fish trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('562', 'prizetrophy4*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '562', 'Fish trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('563', 'prizetrophy4*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '563', 'Fish trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('564', 'prizetrophy*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '564', 'Classic trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('565', 'prizetrophy*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '565', 'Classic trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('566', 'prizetrophy*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '566', 'Classic trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('567', 'prizetrophy5*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '567', 'Duo trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('568', 'prizetrophy5*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '568', 'Duo trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('569', 'prizetrophy5*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '569', 'Duo trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('570', 'prizetrophy2*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '570', 'Duck trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('571', 'prizetrophy2*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '43', '571', 'Duck trophy', 'Shiny silver', '10');
INSERT INTO `catalogue_items` VALUES ('572', 'prizetrophy2*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#996600', '0', '1', '1', '43', '572', 'Duck trophy', 'Breathtaking bronze', '8');
INSERT INTO `catalogue_items` VALUES ('573', 'prizetrophy8*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '573', 'Duo trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('574', 'prizetrophy9*1', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFDD3F', '0', '1', '1', '43', '574', 'Champion trophy', 'Glittery gold', '12');
INSERT INTO `catalogue_items` VALUES ('575', 'poster 20', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '575', 'Snowman Poster', 'A new use for carrots!', '3');
INSERT INTO `catalogue_items` VALUES ('576', 'poster 21', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '576', 'Angel Poster', 'See that halo gleam!', '3');
INSERT INTO `catalogue_items` VALUES ('577', 'poster 22', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '577', 'Winter Wonderland Poster', 'A chilly snowy scene', '3');
INSERT INTO `catalogue_items` VALUES ('578', 'poster 23', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '578', 'Santa Poster Poster', 'The jolly fat man himself', '3');
INSERT INTO `catalogue_items` VALUES ('579', 'poster 24', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '579', 'Three Wise Men Poster', 'Following the star!', '3');
INSERT INTO `catalogue_items` VALUES ('580', 'poster 25', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '580', 'Reindeer Poster', 'Doing a hard night\'s work', '3');
INSERT INTO `catalogue_items` VALUES ('581', 'poster 26', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '581', 'Stocking', 'Hung yours up yet?', '3');
INSERT INTO `catalogue_items` VALUES ('582', 'poster 27', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '582', 'Holly Garland', 'Deck the halls!', '3');
INSERT INTO `catalogue_items` VALUES ('583', 'poster 28', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '583', 'Tinsel (silver)', 'A touch of festive sparkle', '3');
INSERT INTO `catalogue_items` VALUES ('584', 'poster 29', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '584', 'Tinsel (gold)', 'A touch of festive sparkle', '3');
INSERT INTO `catalogue_items` VALUES ('585', 'poster 30', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '585', 'Mistletoe', 'Pucker up', '3');
INSERT INTO `catalogue_items` VALUES ('586', 'poster 46', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '586', 'Small gold star', 'Twinkle, twinkle', '3');
INSERT INTO `catalogue_items` VALUES ('587', 'poster 47', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '587', 'Small silver star', 'Twinkle, twinkle', '3');
INSERT INTO `catalogue_items` VALUES ('588', 'poster 48', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '588', 'Large gold star', 'All that glitters...', '3');
INSERT INTO `catalogue_items` VALUES ('589', 'poster 49', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '44', '589', 'Large silver star', 'All that glitters...', '3');
INSERT INTO `catalogue_items` VALUES ('590', 'tree3', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '590', 'Christmas Tree 1', 'Any presents under it yet?', '6');
INSERT INTO `catalogue_items` VALUES ('591', 'tree4', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '591', 'Christmas Tree 2', 'Any presents under it yet?', '6');
INSERT INTO `catalogue_items` VALUES ('592', 'tree5', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '592', 'Christmas Tree 3', 'Any presents under it yet?', '6');
INSERT INTO `catalogue_items` VALUES ('593', 'triplecandle', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '593', 'Electric Candles', 'No need to worry about wax drips', '3');
INSERT INTO `catalogue_items` VALUES ('594', 'turkey', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '594', 'Roast Turkey', 'Where\'s the cranberry sauce?', '3');
INSERT INTO `catalogue_items` VALUES ('595', 'house', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '595', 'Gingerbread House', 'Good enough to eat', '3');
INSERT INTO `catalogue_items` VALUES ('596', 'pudding', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '596', 'Christmas Pudding', 'Will you get the lucky sixpence?', '3');
INSERT INTO `catalogue_items` VALUES ('597', 'xmasduck', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '597', 'Christmas Rubber Duck', 'A right Christmas quacker!', '2');
INSERT INTO `catalogue_items` VALUES ('598', 'hyacinth1', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '598', 'Pink Hyacinth', 'Beautiful bulb', '3');
INSERT INTO `catalogue_items` VALUES ('599', 'hyacinth2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '599', 'Blue Hyacinth', 'Beautiful bulb', '3');
INSERT INTO `catalogue_items` VALUES ('600', 'joulutahti', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '600', 'Poinsetta', 'Christmas in a pot', '3');
INSERT INTO `catalogue_items` VALUES ('601', 'rcandle', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '601', 'Red Candle', 'Xmas tea light', '3');
INSERT INTO `catalogue_items` VALUES ('602', 'wcandle', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '44', '602', 'White Candle', 'Xmas tea light', '3');
INSERT INTO `catalogue_items` VALUES ('603', 'easterduck', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '45', '603', 'Wannabe bunny', 'Can you tell what it is yet?', '2');
INSERT INTO `catalogue_items` VALUES ('604', 'birdie', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '45', '604', 'Pop-up Egg', 'Cheep (!) and cheerful', '5');
INSERT INTO `catalogue_items` VALUES ('605', 'basket', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '45', '605', 'Basket Of Eggs', 'Eggs-actly what you want for Easter', '4');
INSERT INTO `catalogue_items` VALUES ('606', 'bunny', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '45', '606', 'Squidgy Bunny', 'Yours to cuddle up to', '3');
INSERT INTO `catalogue_items` VALUES ('607', 'pumpkin', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '607', 'Pumpkin Lamp', 'Cast a spooky glow', '6');
INSERT INTO `catalogue_items` VALUES ('608', 'poster 42', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '608', 'Spiderweb', 'Not something you want to run into', '3');
INSERT INTO `catalogue_items` VALUES ('609', 'poster 43', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '609', 'Chains', 'Shake, rattle and roll!', '4');
INSERT INTO `catalogue_items` VALUES ('610', 'skullcandle', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '610', 'Skull Candle Holder', 'Alas poor Yorrick...', '4');
INSERT INTO `catalogue_items` VALUES ('611', 'poster 45', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '611', 'Skeleton', 'Needs a few more Habburgers', '3');
INSERT INTO `catalogue_items` VALUES ('612', 'poster 44', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '612', 'Mummy', 'Beware the curse...', '3');
INSERT INTO `catalogue_items` VALUES ('613', 'deadduck', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '613', 'Dead Duck', 'Blood, but no guts', '2');
INSERT INTO `catalogue_items` VALUES ('614', 'deadduck2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '614', 'Dead Duck 2', 'Someone forgot to feed me...', '2');
INSERT INTO `catalogue_items` VALUES ('615', 'deadduck3', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '615', 'Dead Duck 3', 'With added ectoplasm', '2');
INSERT INTO `catalogue_items` VALUES ('616', 'poster 50', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '616', 'Bat Poster', 'flap, flap, screech, screech...', '3');
INSERT INTO `catalogue_items` VALUES ('617', 'poster 501', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '46', '617', 'Jolly Roger', 'For pirates everywhere', '3');
INSERT INTO `catalogue_items` VALUES ('618', 'ham2', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '618', 'Eaten Ham', 'Looks like you\'re too late!', '3');
INSERT INTO `catalogue_items` VALUES ('619', 'habboween_crypt', '1', '2', '2', '1.00', '0,0,0', '0', '1', '1', '46', '619', 'HabboWheen Crypt', 'HabboWheen Crypt', '5');
INSERT INTO `catalogue_items` VALUES ('625', 'habboween_grass', '4', '2', '2', '1.00', '0,0,0', '0', '1', '1', '46', '625', 'HabboWheen Grass', 'HabboWheen Grass', '5');
INSERT INTO `catalogue_items` VALUES ('620', 'valeduck', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '47', '620', 'Valentine\'s Duck', 'He\'s lovestruck', '2');
INSERT INTO `catalogue_items` VALUES ('621', 'hal_cauldron', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '621', 'HabboWheen Cauldron', 'HabboWheen Cauldron', '5');
INSERT INTO `catalogue_items` VALUES ('622', 'hal_grave', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '46', '622', 'HabboWheen Grave', 'HabboWheen Grave', '5');
INSERT INTO `catalogue_items` VALUES ('623', 'heartsofa', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '47', '623', 'Heart Sofa', 'Perfect for snuggling up on', '5');
INSERT INTO `catalogue_items` VALUES ('624', 'statue', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '47', '624', 'Cupid Statue', 'Watch out for those arrows!', '3');
INSERT INTO `catalogue_items` VALUES ('626', 'heart', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '47', '626', 'Giant Heart', 'Full of love', '6');
INSERT INTO `catalogue_items` VALUES ('627', 'post.it.vd', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '47', '627', 'Heart stickies', 'Heart stickies', '3');
INSERT INTO `catalogue_items` VALUES ('628', 'carpet_valentine', '4', '2', '7', '0.00', '0,0,0', '0', '1', '1', '47', '628', 'Red Carpet', 'For making an entrance.', '15');
INSERT INTO `catalogue_items` VALUES ('629', 'val_heart', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '47', '629', 'Valentine\'s lamp', 'Valentine\'s lamp', '10');
INSERT INTO `catalogue_items` VALUES ('630', 'plant_valentinerose*1', '1', '1', '1', '1.00', '#FFFFFF,#FF1E1E,#FFFFFF,#FFFFFF', '0', '1', '1', '47', '630', 'Valentine rose Red', 'For a love that it true', '8');
INSERT INTO `catalogue_items` VALUES ('631', 'plant_valentinerose*2', '1', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '47', '631', 'White Valentine Rose', 'Your secret love', '8');
INSERT INTO `catalogue_items` VALUES ('632', 'plant_valentinerose*3', '1', '1', '1', '1.00', '#FFFFFF,#ffee00,#FFFFFF,#FFFFFF', '0', '1', '1', '47', '632', 'Yellow Valentine Rose', 'Relight your passions', '8');
INSERT INTO `catalogue_items` VALUES ('633', 'plant_valentinerose*4', '1', '1', '1', '1.00', '#FFFFFF,#ffbbcf,#FFFFFF,#FFFFFF', '0', '1', '1', '47', '633', 'Pink Valentine\'s Rose', 'Be mine!', '8');
INSERT INTO `catalogue_items` VALUES ('634', 'plant_valentinerose*5', '1', '1', '1', '1.00', '#FFFFFF,#CC3399,#FFFFFF,#FFFFFF', '0', '1', '1', '47', '634', 'Purple Valentine Rose', 'For that special pixel', '8');
INSERT INTO `catalogue_items` VALUES ('635', 'sofa_silo*2', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#525252,#525252,#525252,#525252', '0', '1', '1', '48', '635', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('636', 'sofachair_silo*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '48', '636', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('637', 'table_silo_small*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '48', '637', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('638', 'divider_silo3*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#525252', '1', '1', '1', '48', '638', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('639', 'divider_silo1*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '48', '639', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('640', 'chair_silo*2', '2', '1', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '48', '640', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('641', 'safe_silo*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '48', '641', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('642', 'barchair_silo*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '48', '642', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('643', 'table_silo_med*2', '1', '2', '2', '1.00', '#ffffff,#525252', '0', '1', '1', '48', '643', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('644', 'sofa_silo*3', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '48', '644', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('645', 'sofachair_silo*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFFFFF,#FFFFFF', '0', '1', '1', '48', '645', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('646', 'table_silo_small*3', '1', '1', '1', '1.00', '#ffffff,#FFFFFF', '0', '1', '1', '48', '646', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('647', 'divider_silo3*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#FFFFFF', '1', '1', '1', '48', '647', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('648', 'divider_silo1*3', '1', '1', '1', '1.00', '#ffffff,#FFFFFF', '0', '1', '1', '48', '648', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('649', 'chair_silo*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#FFFFFF,#FFFFFF', '0', '1', '1', '48', '649', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('650', 'safe_silo*3', '1', '1', '1', '1.00', '#ffffff,#FFFFFF', '0', '1', '1', '48', '650', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('651', 'barchair_silo*3', '1', '1', '1', '1.00', '#ffffff,#FFFFFF', '0', '1', '1', '48', '651', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('652', 'table_silo_med*3', '1', '2', '2', '1.00', '#ffffff,#FFFFFF', '0', '1', '1', '48', '652', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('653', 'sofa_silo*4', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '48', '653', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('654', 'sofachair_silo*4', '1', '1', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '48', '654', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('655', 'table_silo_small*4', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '48', '655', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('656', 'divider_silo3*4', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#F7EBBC', '1', '1', '1', '48', '656', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('657', 'divider_silo1*4', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '48', '657', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('658', 'chair_silo*4', '2', '1', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '48', '658', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('659', 'safe_silo*4', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '48', '659', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('660', 'barchair_silo*4', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '48', '660', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('661', 'table_silo_med*4', '1', '2', '2', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '48', '661', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('662', 'sofa_silo*5', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#FF99BC,#FF99BC,#FF99BC,#FF99BC', '0', '1', '1', '48', '662', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('663', 'sofachair_silo*5', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FF99BC,#FF99BC', '0', '1', '1', '48', '663', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('664', 'table_silo_small*5', '1', '1', '1', '1.00', '#ffffff,#FF99BC', '0', '1', '1', '48', '664', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('665', 'divider_silo3*5', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#FF99BC', '1', '1', '1', '48', '665', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('666', 'divider_silo1*5', '1', '1', '1', '1.00', '#ffffff,#FF99BC', '0', '1', '1', '48', '666', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('667', 'chair_silo*5', '2', '1', '1', '1.00', '#ffffff,#ffffff,#FF99BC,#FF99BC', '0', '1', '1', '48', '667', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('668', 'safe_silo*5', '1', '1', '1', '1.00', '#ffffff,#FF99BC', '0', '1', '1', '48', '668', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('669', 'barchair_silo*5', '1', '1', '1', '1.00', '#ffffff,#FF99BC', '0', '1', '1', '48', '669', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('670', 'table_silo_med*5', '1', '2', '2', '1.00', '#ffffff,#FF99BC', '0', '1', '1', '48', '670', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('671', 'sofa_silo*6', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '48', '671', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('672', 'sofachair_silo*6', '1', '1', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '48', '672', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('673', 'table_silo_small*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '48', '673', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('674', 'divider_silo3*6', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#5EAAF8', '1', '1', '1', '48', '674', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('675', 'divider_silo1*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '48', '675', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('676', 'chair_silo*6', '2', '1', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '48', '676', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('677', 'safe_silo*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '48', '677', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('678', 'barchair_silo*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '48', '678', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('679', 'table_silo_med*6', '1', '2', '2', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '48', '679', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('680', 'sofa_silo*7', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#92D13D,#92D13D,#92D13D,#92D13D', '0', '1', '1', '48', '680', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('681', 'sofachair_silo*7', '1', '1', '1', '1.00', '#ffffff,#ffffff,#92D13D,#92D13D', '0', '1', '1', '48', '681', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('682', 'table_silo_small*7', '1', '1', '1', '1.00', '#ffffff,#92D13D', '0', '1', '1', '48', '682', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('683', 'divider_silo3*7', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#92D13D', '1', '1', '1', '48', '683', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('684', 'divider_silo1*7', '1', '1', '1', '1.00', '#ffffff,#92D13D', '0', '1', '1', '48', '684', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('685', 'chair_silo*7', '2', '1', '1', '1.00', '#ffffff,#ffffff,#92D13D,#92D13D', '0', '1', '1', '48', '685', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('686', 'safe_silo*7', '1', '1', '1', '1.00', '#ffffff,#92D13D', '0', '1', '1', '48', '686', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('687', 'barchair_silo*7', '1', '1', '1', '1.00', '#ffffff,#92D13D', '0', '1', '1', '48', '687', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('688', 'table_silo_med*7', '1', '2', '2', '1.00', '#ffffff,#92D13D', '0', '1', '1', '48', '688', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('689', 'sofa_silo*8', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#FFD837,#FFD837,#FFD837,#FFD837', '0', '1', '1', '48', '689', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('690', 'sofachair_silo*8', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '48', '690', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('691', 'table_silo_small*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '48', '691', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('692', 'divider_silo3*8', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#FFD837', '1', '1', '1', '48', '692', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('693', 'divider_silo1*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '48', '693', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('694', 'chair_silo*8', '2', '1', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '48', '694', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('695', 'safe_silo*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '48', '695', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('696', 'barchair_silo*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '48', '696', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('697', 'table_silo_med*8', '1', '2', '2', '1.00', '#ffffff,#FFD837', '0', '1', '1', '48', '697', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('698', 'sofa_silo*9', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#E14218,#E14218,#E14218,#E14218', '0', '1', '1', '48', '698', 'Two-Seater Sofa', 'Cushioned, understated comfort', '3');
INSERT INTO `catalogue_items` VALUES ('699', 'sofachair_silo*9', '1', '1', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '48', '699', 'Armchair', 'Large, but worth it', '3');
INSERT INTO `catalogue_items` VALUES ('700', 'table_silo_small*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '48', '700', 'Occasional Table', 'For those random moments', '1');
INSERT INTO `catalogue_items` VALUES ('701', 'divider_silo3*9', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#E14218', '1', '1', '1', '48', '701', 'Gate (lockable)', 'Form following function', '6');
INSERT INTO `catalogue_items` VALUES ('702', 'divider_silo1*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '48', '702', 'Corner Shelf', 'Neat and natty', '3');
INSERT INTO `catalogue_items` VALUES ('703', 'chair_silo*9', '2', '1', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '48', '703', 'Dining Chair', 'Keep it simple', '3');
INSERT INTO `catalogue_items` VALUES ('704', 'safe_silo*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '48', '704', 'Safe Minibar', 'Totally shatter-proof!', '3');
INSERT INTO `catalogue_items` VALUES ('705', 'barchair_silo*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '48', '705', 'Bar Stool', 'Practical and convenient', '3');
INSERT INTO `catalogue_items` VALUES ('706', 'table_silo_med*9', '1', '2', '2', '1.00', '#ffffff,#E14218', '0', '1', '1', '48', '706', 'Coffee Table', 'Wipe clean and unobtrusive', '3');
INSERT INTO `catalogue_items` VALUES ('707', 'chair_norja*2', '2', '1', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '49', '707', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('708', 'couch_norja*2', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#525252,#525252,#525252,#525252', '0', '1', '1', '49', '708', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('709', 'table_norja_med*2', '1', '2', '2', '1.00', '#ffffff,#525252', '0', '1', '1', '49', '709', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('710', 'shelves_norja*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '49', '710', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('711', 'soft_sofachair_norja*2', '2', '1', '1', '1.00', '#ffffff,#525252,#525252', '0', '1', '1', '49', '711', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('712', 'soft_sofa_norja*2', '2', '2', '1', '1.00', '#ffffff,#525252,#ffffff,#525252,#525252,#525252', '0', '1', '1', '49', '712', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('713', 'divider_nor2*2', '1', '2', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '49', '713', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('714', 'divider_nor1*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '49', '714', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('715', 'divider_nor3*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '1', '1', '1', '49', '715', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('716', 'divider_nor4*2', '1', '2', '1', '1.00', '#ffffff,#ffffff,#525252,#525252,#525252,#525252', '1', '1', '1', '49', '716', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('717', 'divider_nor5*2', '1', '1', '1', '1.00', '#ffffff,#525252,#525252,#525252,#525252,#525252', '1', '1', '1', '49', '717', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('718', 'chair_norja*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '49', '718', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('719', 'couch_norja*3', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '49', '719', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('720', 'table_norja_med*3', '1', '2', '2', '1.00', '#ffffff,#ffffff', '0', '1', '1', '49', '720', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('721', 'shelves_norja*3', '1', '1', '1', '1.00', '#ffffff,#ffffff', '0', '1', '1', '49', '721', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('722', 'soft_sofachair_norja*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '49', '722', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('723', 'soft_sofa_norja*3', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '49', '723', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('724', 'divider_nor2*3', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '49', '724', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('725', 'divider_nor1*3', '1', '1', '1', '1.00', '#ffffff,#ffffff', '0', '1', '1', '49', '725', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('726', 'divider_nor3*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '1', '1', '1', '49', '726', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('727', 'divider_nor4*3', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '1', '1', '1', '49', '727', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('728', 'divider_nor5*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '1', '1', '1', '49', '728', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('729', 'chair_norja*4', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '49', '729', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('730', 'couch_norja*4', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ABD0D2,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '49', '730', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('731', 'table_norja_med*4', '1', '2', '2', '1.00', '#ffffff,#ABD0D2', '0', '1', '1', '49', '731', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('732', 'shelves_norja*4', '1', '1', '1', '1.00', '#ffffff,#ABD0D2', '0', '1', '1', '49', '732', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('733', 'soft_sofachair_norja*4', '2', '1', '1', '1.00', '#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '49', '733', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('734', 'soft_sofa_norja*4', '2', '2', '1', '1.00', '#ffffff,#ABD0D2,#ffffff,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '49', '734', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('735', 'divider_nor2*4', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '0', '1', '1', '49', '735', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('736', 'divider_nor1*4', '1', '1', '1', '1.00', '#ffffff,#ABD0D2', '0', '1', '1', '49', '736', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('737', 'divider_nor3*4', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2', '1', '1', '1', '49', '737', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('738', 'divider_nor4*4', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ABD0D2,#ABD0D2,#ABD0D2,#ABD0D2', '1', '1', '1', '49', '738', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('739', 'divider_nor5*4', '1', '1', '1', '1.00', '#ffffff,#ABD0D2,#ABD0D2,#ABD0D2,#ABD0D2,#ABD0D2', '1', '1', '1', '49', '739', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('740', 'chair_norja*5', '2', '1', '1', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '49', '740', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('741', 'couch_norja*5', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#EE7EA4,#EE7EA4,#EE7EA4,#EE7EA4', '0', '1', '1', '49', '741', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('742', 'table_norja_med*5', '1', '2', '2', '1.00', '#ffffff,#EE7EA4', '0', '1', '1', '49', '742', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('743', 'shelves_norja*5', '1', '1', '1', '1.00', '#ffffff,#EE7EA4', '0', '1', '1', '49', '743', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('744', 'soft_sofachair_norja*5', '2', '1', '1', '1.00', '#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '49', '744', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('745', 'soft_sofa_norja*5', '2', '2', '1', '1.00', '#ffffff,#EE7EA4,#ffffff,#EE7EA4,#EE7EA4,#EE7EA4', '0', '1', '1', '49', '745', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('746', 'divider_nor2*5', '1', '2', '1', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4', '0', '1', '1', '49', '746', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('747', 'divider_nor1*5', '1', '1', '1', '1.00', '#ffffff,#EE7EA4', '0', '1', '1', '49', '747', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('748', 'divider_nor3*5', '1', '1', '1', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4', '1', '1', '1', '49', '748', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('749', 'divider_nor4*5', '1', '2', '1', '1.00', '#ffffff,#ffffff,#EE7EA4,#EE7EA4,#EE7EA4,#EE7EA4', '1', '1', '1', '49', '749', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('750', 'divider_nor5*5', '1', '1', '1', '1.00', '#ffffff,#EE7EA4,#EE7EA4,#EE7EA4,#EE7EA4,#EE7EA4', '1', '1', '1', '49', '750', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('751', 'chair_norja*6', '2', '1', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '49', '751', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('752', 'couch_norja*6', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '49', '752', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('753', 'table_norja_med*6', '1', '2', '2', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '49', '753', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('754', 'shelves_norja*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '49', '754', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('755', 'soft_sofachair_norja*6', '2', '1', '1', '1.00', '#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '49', '755', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('756', 'soft_sofa_norja*6', '2', '2', '1', '1.00', '#ffffff,#5EAAF8,#ffffff,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '49', '756', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('757', 'divider_nor2*6', '1', '2', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '49', '757', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('758', 'divider_nor1*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '49', '758', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('759', 'divider_nor3*6', '1', '1', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '1', '1', '1', '49', '759', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('760', 'divider_nor4*6', '1', '2', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8', '1', '1', '1', '49', '760', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('761', 'divider_nor5*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8', '1', '1', '1', '49', '761', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('762', 'chair_norja*7', '2', '1', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '49', '762', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('763', 'couch_norja*7', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#7CB135,#7CB135,#7CB135,#7CB135', '0', '1', '1', '49', '763', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('764', 'table_norja_med*7', '1', '2', '2', '1.00', '#ffffff,#7CB135', '0', '1', '1', '49', '764', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('765', 'shelves_norja*7', '1', '1', '1', '1.00', '#ffffff,#7CB135', '0', '1', '1', '49', '765', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('766', 'soft_sofachair_norja*7', '2', '1', '1', '1.00', '#ffffff,#7CB135,#7CB135', '0', '1', '1', '49', '766', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('767', 'soft_sofa_norja*7', '2', '2', '1', '1.00', '#ffffff,#7CB135,#ffffff,#7CB135,#7CB135,#7CB135', '0', '1', '1', '49', '767', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('768', 'divider_nor2*7', '1', '2', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '49', '768', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('769', 'divider_nor1*7', '1', '1', '1', '1.00', '#ffffff,#7CB135', '0', '1', '1', '49', '769', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('770', 'divider_nor3*7', '1', '1', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '1', '1', '1', '49', '770', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('771', 'divider_nor4*7', '1', '2', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135,#7CB135,#7CB135', '1', '1', '1', '49', '771', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('772', 'divider_nor5*7', '1', '1', '1', '1.00', '#ffffff,#7CB135,#7CB135,#7CB135,#7CB135,#7CB135', '1', '1', '1', '49', '772', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('773', 'chair_norja*8', '2', '1', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '49', '773', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('774', 'couch_norja*8', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#FFD837,#FFD837,#FFD837,#FFD837', '0', '1', '1', '49', '774', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('775', 'table_norja_med*8', '1', '2', '2', '1.00', '#ffffff,#FFD837', '0', '1', '1', '49', '775', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('776', 'shelves_norja*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '49', '776', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('777', 'soft_sofachair_norja*8', '2', '1', '1', '1.00', '#ffffff,#FFD837,#FFD837', '0', '1', '1', '49', '777', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('778', 'soft_sofa_norja*8', '2', '2', '1', '1.00', '#ffffff,#FFD837,#ffffff,#FFD837,#FFD837,#FFD837', '0', '1', '1', '49', '778', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('779', 'divider_nor2*8', '1', '2', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '49', '779', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('780', 'divider_nor1*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '49', '780', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('781', 'divider_nor3*8', '1', '1', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '1', '1', '1', '49', '781', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('782', 'divider_nor4*8', '1', '2', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837,#FFD837,#FFD837', '1', '1', '1', '49', '782', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('783', 'divider_nor5*8', '1', '1', '1', '1.00', '#ffffff,#FFD837,#FFD837,#FFD837,#FFD837,#FFD837', '1', '1', '1', '49', '783', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('784', 'chair_norja*9', '2', '1', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '49', '784', 'Chair', 'Sleek and chic for each cheek', '3');
INSERT INTO `catalogue_items` VALUES ('785', 'couch_norja*9', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#E14218,#E14218,#E14218,#E14218', '0', '1', '1', '49', '785', 'Bench', 'Two can perch comfortably', '3');
INSERT INTO `catalogue_items` VALUES ('786', 'table_norja_med*9', '1', '2', '2', '1.00', '#ffffff,#E14218', '0', '1', '1', '49', '786', 'Coffee Table', 'Elegance embodied', '3');
INSERT INTO `catalogue_items` VALUES ('787', 'shelves_norja*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '49', '787', 'Bookcase', 'For nic naks and art deco books', '3');
INSERT INTO `catalogue_items` VALUES ('788', 'soft_sofachair_norja*9', '2', '1', '1', '1.00', '#ffffff,#E14218,#E14218', '0', '1', '1', '49', '788', 'iced sofachair', 'Soft iced sofachair', '3');
INSERT INTO `catalogue_items` VALUES ('789', 'soft_sofa_norja*9', '2', '2', '1', '1.00', '#ffffff,#E14218,#ffffff,#E14218,#E14218,#E14218', '0', '1', '1', '49', '789', 'iced sofa', 'A soft iced sofa', '4');
INSERT INTO `catalogue_items` VALUES ('790', 'divider_nor2*9', '1', '2', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '49', '790', 'Ice Bar-Desk', 'Strong, yet soft looking', '3');
INSERT INTO `catalogue_items` VALUES ('791', 'divider_nor1*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '49', '791', 'Ice Corner', 'Looks squishy, but isn\'t', '3');
INSERT INTO `catalogue_items` VALUES ('792', 'divider_nor3*9', '1', '1', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '1', '1', '1', '49', '792', 'Door (Lockable)', 'Do go through...', '6');
INSERT INTO `catalogue_items` VALUES ('793', 'divider_nor4*9', '1', '2', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218,#E14218,#E14218', '1', '1', '1', '49', '793', 'Iced Auto Shutter', 'Habbos, roll out!', '3');
INSERT INTO `catalogue_items` VALUES ('794', 'divider_nor5*9', '1', '1', '1', '1.00', '#ffffff,#E14218,#E14218,#E14218,#E14218,#E14218', '1', '1', '1', '49', '794', 'Iced Angle', 'Cool cornering for you!', '3');
INSERT INTO `catalogue_items` VALUES ('795', 'bed_polyfon*2', '2', '2', '3', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '50', '795', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('796', 'bed_polyfon_one*2', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '50', '796', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('797', 'sofa_polyfon*2', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#525252,#525252,#525252,#525252', '0', '1', '1', '50', '797', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('798', 'sofachair_polyfon*2', '2', '1', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '50', '798', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('799', 'divider_poly3*2', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#525252,#525252', '1', '1', '1', '50', '799', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('800', 'bardesk_polyfon*2', '1', '2', '1', '1.00', '#ffffff,#ffffff,#525252,#525252', '0', '1', '1', '50', '800', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('801', 'bardeskcorner_polyfon*2', '1', '1', '1', '1.00', '#ffffff,#525252', '0', '1', '1', '50', '801', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('802', 'bed_polyfon*3', '2', '2', '3', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '50', '802', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('803', 'bed_polyfon_one*3', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '50', '803', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('804', 'sofa_polyfon*3', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '50', '804', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('805', 'sofachair_polyfon*3', '2', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '50', '805', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('806', 'divider_poly3*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '1', '1', '1', '50', '806', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('807', 'bardesk_polyfon*3', '1', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff', '0', '1', '1', '50', '807', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('808', 'bardeskcorner_polyfon*3', '1', '1', '1', '1.00', '#ffffff,#ffffff', '0', '1', '1', '50', '808', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('809', 'bed_polyfon*4', '2', '2', '3', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '50', '809', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('810', 'bed_polyfon_one*4', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '50', '810', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('811', 'sofa_polyfon*4', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#F7EBBC,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '50', '811', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('812', 'sofachair_polyfon*4', '2', '1', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '50', '812', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('813', 'divider_poly3*4', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#F7EBBC,#F7EBBC', '1', '1', '1', '50', '813', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('814', 'bardesk_polyfon*4', '1', '2', '1', '1.00', '#ffffff,#ffffff,#F7EBBC,#F7EBBC', '0', '1', '1', '50', '814', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('815', 'bardeskcorner_polyfon*4', '1', '1', '1', '1.00', '#ffffff,#F7EBBC', '0', '1', '1', '50', '815', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('816', 'bed_polyfon*6', '2', '2', '3', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '50', '816', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('817', 'bed_polyfon_one*6', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '50', '817', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('818', 'sofa_polyfon*6', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#5EAAF8,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '50', '818', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('819', 'sofachair_polyfon*6', '2', '1', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '50', '819', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('820', 'divider_poly3*6', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#5EAAF8,#5EAAF8', '1', '1', '1', '50', '820', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('821', 'bardesk_polyfon*6', '1', '2', '1', '1.00', '#ffffff,#ffffff,#5EAAF8,#5EAAF8', '0', '1', '1', '50', '821', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('822', 'bardeskcorner_polyfon*6', '1', '1', '1', '1.00', '#ffffff,#5EAAF8', '0', '1', '1', '50', '822', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('823', 'bed_polyfon*7', '2', '2', '3', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '50', '823', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('824', 'bed_polyfon_one*7', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '50', '824', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('825', 'sofa_polyfon*7', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#7CB135,#7CB135,#7CB135,#7CB135', '0', '1', '1', '50', '825', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('826', 'sofachair_polyfon*7', '2', '1', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '50', '826', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('827', 'divider_poly3*7', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#7CB135,#7CB135', '1', '1', '1', '50', '827', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('828', 'bardesk_polyfon*7', '1', '2', '1', '1.00', '#ffffff,#ffffff,#7CB135,#7CB135', '0', '1', '1', '50', '828', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('829', 'bardeskcorner_polyfon*7', '1', '1', '1', '1.00', '#ffffff,#7CB135', '0', '1', '1', '50', '829', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('830', 'bed_polyfon*8', '2', '2', '3', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '50', '830', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('831', 'bed_polyfon_one*8', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '50', '831', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('832', 'sofa_polyfon*8', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#FFD837,#FFD837,#FFD837,#FFD837', '0', '1', '1', '50', '832', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('833', 'sofachair_polyfon*8', '2', '1', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '50', '833', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('834', 'divider_poly3*8', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#FFD837,#FFD837', '1', '1', '1', '50', '834', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('835', 'bardesk_polyfon*8', '1', '2', '1', '1.00', '#ffffff,#ffffff,#FFD837,#FFD837', '0', '1', '1', '50', '835', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('836', 'bardeskcorner_polyfon*8', '1', '1', '1', '1.00', '#ffffff,#FFD837', '0', '1', '1', '50', '836', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('837', 'bed_polyfon*9', '2', '2', '3', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '50', '837', 'Double Bed', 'Give yourself space to stretch out', '4');
INSERT INTO `catalogue_items` VALUES ('838', 'bed_polyfon_one*9', '2', '1', '3', '1.00', '#ffffff,#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '50', '838', 'Single Bed', 'Cot of the artistic', '3');
INSERT INTO `catalogue_items` VALUES ('839', 'sofa_polyfon*9', '2', '2', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#E14218,#E14218,#E14218,#E14218', '0', '1', '1', '50', '839', 'Two-seater Sofa', 'Comfort for stylish couples', '4');
INSERT INTO `catalogue_items` VALUES ('840', 'sofachair_polyfon*9', '2', '1', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '50', '840', 'Armchair', 'Loft-style comfort', '3');
INSERT INTO `catalogue_items` VALUES ('841', 'divider_poly3*9', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#E14218,#E14218', '1', '1', '1', '50', '841', 'Hatch (Lockable)', 'All bars should have one', '6');
INSERT INTO `catalogue_items` VALUES ('842', 'bardesk_polyfon*9', '1', '2', '1', '1.00', '#ffffff,#ffffff,#E14218,#E14218', '0', '1', '1', '50', '842', 'Bar/desk', 'Perfect for work or play', '3');
INSERT INTO `catalogue_items` VALUES ('843', 'bardeskcorner_polyfon*9', '1', '1', '1', '1.00', '#ffffff,#E14218', '0', '1', '1', '50', '843', 'Corner Cabinet/Desk', 'Tuck it away', '3');
INSERT INTO `catalogue_items` VALUES ('844', 'pura_mdl1*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#ABD0D2,#FFFFFF', '0', '1', '1', '51', '844', 'Aqua Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('845', 'pura_mdl2*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#ABD0D2,#FFFFFF', '0', '1', '1', '51', '845', 'Aqua Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('846', 'pura_mdl3*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#ABD0D2,#FFFFFF', '0', '1', '1', '51', '846', 'Aqua Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('847', 'pura_mdl4*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#ABD0D2,#ABD0D2', '0', '1', '1', '51', '847', 'Aqua Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('848', 'pura_mdl5*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#FFFFFF', '0', '1', '1', '51', '848', 'Aqua Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('849', 'pura_mdl1*2', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FF99BC,#FFFFFF', '0', '1', '1', '51', '849', 'Pink Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('850', 'pura_mdl2*2', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FF99BC,#FFFFFF', '0', '1', '1', '51', '850', 'Pink Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('851', 'pura_mdl3*2', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FF99BC,#FFFFFF', '0', '1', '1', '51', '851', 'Pink Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('852', 'pura_mdl4*2', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FF99BC,#FF99BC', '0', '1', '1', '51', '852', 'Pink Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('853', 'pura_mdl5*2', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FF99BC,#FFFFFF', '0', '1', '1', '51', '853', 'Pink Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('854', 'pura_mdl1*3', '2', '1', '1', '1.00', '#FFFFFF,#525252,#525252,#FFFFFF', '0', '1', '1', '51', '854', 'Black Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('855', 'pura_mdl2*3', '2', '1', '1', '1.00', '#FFFFFF,#525252,#525252,#FFFFFF', '0', '1', '1', '51', '855', 'Black Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('856', 'pura_mdl3*3', '2', '1', '1', '1.00', '#FFFFFF,#525252,#525252,#FFFFFF', '0', '1', '1', '51', '856', 'Black Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('857', 'pura_mdl4*3', '2', '1', '1', '1.00', '#FFFFFF,#525252,#525252,#525252', '0', '1', '1', '51', '857', 'Black Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('858', 'pura_mdl5*3', '2', '1', '1', '1.00', '#FFFFFF,#525252,#525252,#FFFFFF', '0', '1', '1', '51', '858', 'Black Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('859', 'pura_mdl1*4', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '859', 'White Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('860', 'pura_mdl2*4', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '860', 'White Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('861', 'pura_mdl3*4', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '861', 'White Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('862', 'pura_mdl4*4', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '862', 'White Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('863', 'pura_mdl5*4', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '863', 'White Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('864', 'pura_mdl1*5', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#F7EBBC,#FFFFFF', '0', '1', '1', '51', '864', 'Beige Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('865', 'pura_mdl2*5', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#F7EBBC,#FFFFFF', '0', '1', '1', '51', '865', 'Beige Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('866', 'pura_mdl3*5', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#F7EBBC,#FFFFFF', '0', '1', '1', '51', '866', 'Beige Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('867', 'pura_mdl4*5', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#F7EBBC,#F7EBBC', '0', '1', '1', '51', '867', 'Beige Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('868', 'pura_mdl5*5', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#F7EBBC,#FFFFFF', '0', '1', '1', '51', '868', 'Beige Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('869', 'pura_mdl1*6', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#5EAAF8,#FFFFFF', '0', '1', '1', '51', '869', 'Blue Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('870', 'pura_mdl2*6', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#5EAAF8,#FFFFFF', '0', '1', '1', '51', '870', 'Blue Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('871', 'pura_mdl3*6', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#5EAAF8,#FFFFFF', '0', '1', '1', '51', '871', 'Blue Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('872', 'pura_mdl4*6', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#5EAAF8,#5EAAF8', '0', '1', '1', '51', '872', 'Blue Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('873', 'pura_mdl5*6', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#5EAAF8,#FFFFFF', '0', '1', '1', '51', '873', 'Blue Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('874', 'pura_mdl1*7', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#92D13D,#FFFFFF', '0', '1', '1', '51', '874', 'Green Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('875', 'pura_mdl2*7', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#92D13D,#FFFFFF', '0', '1', '1', '51', '875', 'Green Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('876', 'pura_mdl3*7', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#92D13D,#FFFFFF', '0', '1', '1', '51', '876', 'Green Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('877', 'pura_mdl4*7', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#92D13D,#92D13D', '0', '1', '1', '51', '877', 'Green Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('878', 'pura_mdl5*7', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#92D13D,#FFFFFF', '0', '1', '1', '51', '878', 'Green Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('879', 'pura_mdl1*8', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFD837,#FFFFFF', '0', '1', '1', '51', '879', 'Yellow Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('880', 'pura_mdl2*8', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFD837,#FFFFFF', '0', '1', '1', '51', '880', 'Yellow Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('881', 'pura_mdl3*8', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFD837,#FFFFFF', '0', '1', '1', '51', '881', 'Yellow Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('882', 'pura_mdl4*8', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFD837,#FFD837', '0', '1', '1', '51', '882', 'Yellow Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('883', 'pura_mdl5*8', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFD837,#FFFFFF', '0', '1', '1', '51', '883', 'Yellow Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('884', 'pura_mdl1*9', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#E14218,#FFFFFF', '0', '1', '1', '51', '884', 'Red Pura module 1', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('885', 'pura_mdl2*9', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#E14218,#FFFFFF', '0', '1', '1', '51', '885', 'Red Pura module 2', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('886', 'pura_mdl3*9', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#E14218,#FFFFFF', '0', '1', '1', '51', '886', 'Red Pura module 3', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('887', 'pura_mdl4*9', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#E14218,#E14218', '0', '1', '1', '51', '887', 'Red Pura module 4', 'Endless fun!', '2');
INSERT INTO `catalogue_items` VALUES ('888', 'pura_mdl5*9', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#E14218,#FFFFFF', '0', '1', '1', '51', '888', 'Red Pura module 5', 'Endless fun!', '1');
INSERT INTO `catalogue_items` VALUES ('889', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#ABD0D2,#FFFFFF', '0', '1', '1', '51', '889', 'Aqua Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('890', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#FF99BC,#FFFFFF', '0', '1', '1', '51', '890', 'Pink Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('891', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#525252,#FFFFFF', '0', '1', '1', '51', '891', 'Black Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('892', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '51', '892', 'White Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('893', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#F7EBBC,#FFFFFF', '0', '1', '1', '51', '893', 'Beige Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('894', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#5EAAF8,#FFFFFF', '0', '1', '1', '51', '894', 'Blue Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('895', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#92D13D,#FFFFFF', '0', '1', '1', '51', '895', 'Green Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('896', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#FFD837,#FFFFFF', '0', '1', '1', '51', '896', 'Yellow Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('897', 'chair_basic*1', '2', '1', '1', '1.00', '#FFFFFF,#E14218,#FFFFFF', '0', '1', '1', '51', '897', 'Red Retro Chair', 'Sitback and relax', '5');
INSERT INTO `catalogue_items` VALUES ('898', 'grand_piano*1', '1', '2', '2', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '52', '898', 'Rose Grand Piano', 'Rose Grand Piano', '10');
INSERT INTO `catalogue_items` VALUES ('899', 'romantique_pianochair*1', '2', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '52', '899', 'Rose Quartz Piano Stool', 'Here sat the legend of 1900', '2');
INSERT INTO `catalogue_items` VALUES ('900', 'romantique_chair*1', '2', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '52', '900', 'Rose Quartz Chair', 'Elegant seating for elegant Habbos', '5');
INSERT INTO `catalogue_items` VALUES ('901', 'romantique_divan*1', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FF8B8B', '0', '1', '1', '52', '901', 'Rose Romantique Divan', 'null', '6');
INSERT INTO `catalogue_items` VALUES ('902', 'romantique_divider*1', '1', '2', '1', '1.00', '#FF8B8B,#FF8B8B,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '52', '902', 'Rose Quartz Screen', 'Beauty lies within', '6');
INSERT INTO `catalogue_items` VALUES ('903', 'romantique_smalltabl*1', '1', '1', '1', '1.00', '#FFFFFF,#FF8B8B,#FFFFFF', '0', '1', '1', '52', '903', 'Rose Quartz Tray Table', 'Every tray needs a table...', '4');
INSERT INTO `catalogue_items` VALUES ('904', 'grand_piano*2', '1', '2', '2', '1.00', '#FFFFFF,#A1DC67,#FFFFFF', '0', '1', '1', '52', '904', 'Lime Grand Piano', 'Lime Grand Piano', '10');
INSERT INTO `catalogue_items` VALUES ('905', 'romantique_pianochair*2', '2', '1', '1', '1.00', '#FFFFFF,#A1DC67,#FFFFFF', '0', '1', '1', '52', '905', 'Lime Piano Chair', 'Let the music begin', '2');
INSERT INTO `catalogue_items` VALUES ('906', 'romantique_chair*2', '2', '1', '1', '1.00', '#FFFFFF,#A1DC67,#FFFFFF', '0', '1', '1', '52', '906', 'Lime Romantique Chair', 'null', '5');
INSERT INTO `catalogue_items` VALUES ('907', 'romantique_divan*2', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#A1DC67', '0', '1', '1', '52', '907', 'Emerald Chaise-Longue', 'Recline in continental comfort', '6');
INSERT INTO `catalogue_items` VALUES ('908', 'romantique_divider*2', '1', '2', '1', '1.00', '#A1DC67,#A1DC67,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '52', '908', 'Green Screen', 'Keeping things separated', '6');
INSERT INTO `catalogue_items` VALUES ('909', 'romantique_smalltabl*2', '1', '1', '1', '1.00', '#FFFFFF,#A1DC67,#FFFFFF', '0', '1', '1', '52', '909', 'Lime Tray Table', 'Every tray needs a table...', '4');
INSERT INTO `catalogue_items` VALUES ('910', 'grand_piano*3', '1', '2', '2', '1.00', '#FFFFFF,#5BD1D2,#FFFFFF', '0', '1', '1', '52', '910', 'Pink Grand Piano', 'Make sure you play in key!', '10');
INSERT INTO `catalogue_items` VALUES ('911', 'romantique_pianochair*3', '2', '1', '1', '1.00', '#FFFFFF,#5BD1D2,#FFFFFF', '0', '1', '1', '52', '911', 'Turquoise Piano Chair', 'null', '2');
INSERT INTO `catalogue_items` VALUES ('912', 'romantique_chair*3', '2', '1', '1', '1.00', '#FFFFFF,#5BD1D2,#FFFFFF', '0', '1', '1', '52', '912', 'Turquoise Chair', 'null', '5');
INSERT INTO `catalogue_items` VALUES ('913', 'romantique_divan*3', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#5BD1D2', '0', '1', '1', '52', '913', 'Turquoise Divan', 'null', '6');
INSERT INTO `catalogue_items` VALUES ('914', 'romantique_divider*3', '1', '2', '1', '1.00', '#5BD1D2,#5BD1D2,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '52', '914', 'Turquoise Screen', 'Keeping things separated', '6');
INSERT INTO `catalogue_items` VALUES ('915', 'romantique_smalltabl*3', '1', '1', '1', '1.00', '#FFFFFF,#5BD1D2,#FFFFFF', '0', '1', '1', '52', '915', 'Turquoise Tray Table', 'Every tray needs a table...', '4');
INSERT INTO `catalogue_items` VALUES ('916', 'grand_piano*4', '1', '2', '2', '1.00', '#FFFFFF,#FFC924,#FFFFFF', '0', '1', '1', '52', '916', 'Amber Grand Piano', 'Why is that key green?', '10');
INSERT INTO `catalogue_items` VALUES ('917', 'romantique_pianochair*4', '2', '1', '1', '1.00', '#FFFFFF,#FFC924,#FFFFFF', '0', '1', '1', '52', '917', 'Amber Piano Stool', 'I can feel air coming through...', '2');
INSERT INTO `catalogue_items` VALUES ('918', 'romantique_chair*4', '2', '1', '1', '1.00', '#FFFFFF,#FFC924,#FFFFFF', '0', '1', '1', '52', '918', 'Amber Chair', 'What does this button do?', '5');
INSERT INTO `catalogue_items` VALUES ('919', 'romantique_divan*4', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFC924', '0', '1', '1', '52', '919', 'Amber Chaise-Longue', 'Is that a cape hanging there?', '6');
INSERT INTO `catalogue_items` VALUES ('920', 'romantique_divider*4', '1', '2', '1', '1.00', '#FFC924,#FFC924,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '52', '920', 'Ochre Screen', 'Keeping things separated', '6');
INSERT INTO `catalogue_items` VALUES ('921', 'romantique_smalltabl*4', '1', '1', '1', '1.00', '#FFFFFF,#FFC924,#FFFFFF', '0', '1', '1', '52', '921', 'Amber Tray Table', 'Why is one leg different?', '4');
INSERT INTO `catalogue_items` VALUES ('922', 'grand_piano*5', '1', '2', '2', '1.00', '#FFFFFF,#323C46,#FFFFFF', '0', '1', '1', '52', '922', 'Onyx Grand Piano', 'Why is that key green?', '10');
INSERT INTO `catalogue_items` VALUES ('923', 'romantique_pianochair*5', '2', '1', '1', '1.00', '#FFFFFF,#323C46,#FFFFFF', '0', '1', '1', '52', '923', 'Onyx Piano Stool', 'I can feel air coming through...', '2');
INSERT INTO `catalogue_items` VALUES ('924', 'romantique_chair*5', '2', '1', '1', '1.00', '#FFFFFF,#323C46,#FFFFFF', '0', '1', '1', '52', '924', 'Onyx Chair', 'What does this button do?', '5');
INSERT INTO `catalogue_items` VALUES ('925', 'romantique_divan*5', '2', '2', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#323C46', '0', '1', '1', '52', '925', 'Onyx Chaise-Longue', 'Is that a cape hanging there?', '6');
INSERT INTO `catalogue_items` VALUES ('926', 'romantique_divider*5', '1', '2', '1', '1.00', '#323C46,#323C46,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '52', '926', 'Onyx Quartz Screen', 'Beauty lies within', '6');
INSERT INTO `catalogue_items` VALUES ('927', 'romantique_smalltabl*5', '1', '1', '1', '1.00', '#FFFFFF,#323C46,#FFFFFF', '0', '1', '1', '52', '927', 'Onyx Tray Table', 'Why is one leg different?', '4');
INSERT INTO `catalogue_items` VALUES ('928', 'club_sofa', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '53', '928', 'Club sofa', 'Importants habbos only', '25');
INSERT INTO `catalogue_items` VALUES ('929', 'chair_plasto*14', '2', '1', '1', '1.00', '#ffffff,#3CB4F0,#ffffff,#3CB4F0', '0', '1', '1', '53', '929', 'HC chair', 'Aqua chair', '3');
INSERT INTO `catalogue_items` VALUES ('930', 'table_plasto_4leg*14', '1', '2', '2', '1.00', '#ffffff,#3CB4F0,#ffffff,#3CB4F0', '0', '1', '1', '53', '930', 'HC table', 'Aqua table', '3');
INSERT INTO `catalogue_items` VALUES ('931', 'mocchamaster', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '931', 'Mochamaster', 'Wake up and smell it!', '25');
INSERT INTO `catalogue_items` VALUES ('932', 'edicehc', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '932', 'Dicemaster', 'Click and roll!', '25');
INSERT INTO `catalogue_items` VALUES ('933', 'hcamme', '2', '1', '2', '1.00', '0,0,0', '0', '1', '1', '53', '933', 'Tubmaster', 'Time for a soak', '25');
INSERT INTO `catalogue_items` VALUES ('934', 'doorD', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '934', 'Imperial Teleport', 'Let\'s go over tzar!', '25');
INSERT INTO `catalogue_items` VALUES ('935', 'hcsohva', '2', '2', '1', '1.00', '0,0,0', '0', '1', '1', '53', '935', 'Throne Sofa', 'For royal bottoms...', '25');
INSERT INTO `catalogue_items` VALUES ('936', 'hc_lmp', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '936', 'Oil Lamp', 'Be enlightened', '25');
INSERT INTO `catalogue_items` VALUES ('937', 'hc_tbl', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '53', '937', 'Nordic Table', 'Perfect for banquets', '25');
INSERT INTO `catalogue_items` VALUES ('938', 'hc_chr', '2', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '938', 'Majestic Chair', 'Royal comfort', '25');
INSERT INTO `catalogue_items` VALUES ('939', 'hc_dsk', '1', '1', '2', '1.00', '0,0,0', '0', '1', '1', '53', '939', 'Study Desk', 'For Habbo scholars', '25');
INSERT INTO `catalogue_items` VALUES ('940', 'hc_trll', '1', '1', '2', '1.00', '0,0,0', '0', '1', '1', '53', '940', 'Drinks Trolley', 'For swanky dinners only', '25');
INSERT INTO `catalogue_items` VALUES ('941', 'hc_crpt', '4', '3', '5', '0.00', '0,0,0', '0', '1', '1', '53', '941', 'Persian Carpet', 'Ultimate craftsmanship', '25');
INSERT INTO `catalogue_items` VALUES ('942', 'hc_lmpst', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '942', 'Victorian Street Light', 'Somber and atmospheric', '25');
INSERT INTO `catalogue_items` VALUES ('943', 'hc_crtn', '1', '2', '1', '1.00', '0,0,0', '1', '1', '1', '53', '943', 'Antique Drapery', 'Topnotch privacy protection', '25');
INSERT INTO `catalogue_items` VALUES ('944', 'hc_tv', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '53', '944', 'Mega TV Set', 'Forget plasma, go HC!', '25');
INSERT INTO `catalogue_items` VALUES ('945', 'hc_btlr', '1', '1', '1', '1.00', '0,0,0', '0', '1', '1', '53', '945', 'Electric Butler', 'Your personal caretaker', '25');
INSERT INTO `catalogue_items` VALUES ('946', 'hc_bkshlf', '1', '1', '4', '1.00', '0,0,0', '0', '1', '1', '53', '946', 'Medieval Bookcase', 'For the scholarly ones', '25');
INSERT INTO `catalogue_items` VALUES ('947', 'hc_rntgn', '1', '2', '1', '1.00', '0,0,0', '0', '1', '1', '53', '947', 'X-Ray Divider', 'Believe it or not!', '25');
INSERT INTO `catalogue_items` VALUES ('948', 'hc_machine', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '53', '948', 'Weird Science Machine', 'By and for mad inventors', '25');
INSERT INTO `catalogue_items` VALUES ('949', 'hc_frplc', '1', '1', '3', '1.00', '0,0,0', '0', '1', '1', '53', '949', 'Heavy Duty Fireplace', 'Pixel-powered for maximum heating', '25');
INSERT INTO `catalogue_items` VALUES ('950', 'hc_djset', '1', '3', '1', '1.00', '0,0,0', '0', '1', '1', '53', '950', 'The Grammophon', 'Very old skool scratch\'n\'spin', '25');
INSERT INTO `catalogue_items` VALUES ('951', 'hc_wall_lamp', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '53', '951', 'Retro Wall Lamp', 'Tres chic!', '25');
INSERT INTO `catalogue_items` VALUES ('952', 'roomdimmer', '0', '0', '0', '0.00', '0,0,0', '0', '1', '1', '54', '952', 'Moodlight', 'Turn this off when you\'re gonna bend her over', '25');
INSERT INTO `catalogue_items` VALUES ('953', 'rare_dragonlamp*0', '1', '1', '1', '1.00', '#ffffff,#fa2d00,#fa2d00', '0', '1', '1', '55', '953', 'Fire Dragon Lamp', 'George and the...', '25');
INSERT INTO `catalogue_items` VALUES ('954', 'rare_dragonlamp*1', '1', '1', '1', '1.00', '#ffffff,#3470ff,#3470ff', '0', '1', '1', '55', '954', 'Sea Dragon Lamp', 'Out of the deep blue!', '25');
INSERT INTO `catalogue_items` VALUES ('955', 'rare_dragonlamp*2', '1', '1', '1', '1.00', '#ffffff,#02bb70,#02bb70', '0', '1', '1', '55', '955', 'Jade Dragon Lamp', 'Oriental beast of legends', '25');
INSERT INTO `catalogue_items` VALUES ('956', 'rare_dragonlamp*3', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff', '0', '1', '1', '55', '956', 'Silver Dragon Lamp', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('957', 'rare_dragonlamp*4', '1', '1', '1', '1.00', '#ffffff,#3e3d40,#3e3d40', '0', '1', '1', '55', '957', 'Serpent of Doom', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('958', 'rare_dragonlamp*5', '1', '1', '1', '1.00', '#ffffff,#589a03,#589a03', '0', '1', '1', '55', '958', 'Elf Green Dragon Lamp', 'Roast your chestnuts here!', '25');
INSERT INTO `catalogue_items` VALUES ('959', 'rare_dragonlamp*6', '1', '1', '1', '1.00', '#ffffff,#ffbc00,#ffbc00', '0', '1', '1', '55', '959', 'Gold Dragon Lamp', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('960', 'rare_dragonlamp*7', '1', '1', '1', '1.00', '#ffffff,#83aeff,#83aeff', '0', '1', '1', '55', '960', 'Sky Dragon Lamp', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('961', 'rare_dragonlamp*8', '1', '1', '1', '1.00', '#ffffff,#ff5f01,#ff5f01', '0', '1', '1', '55', '961', 'Bronze Dragon Lamp', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('962', 'rare_dragonlamp*9', '1', '1', '1', '1.00', '#FFFFFF,#B357FF,#B357FF', '0', '1', '1', '55', '962', 'Purple Dragon Lamp', 'Scary and scorching!', '25');
INSERT INTO `catalogue_items` VALUES ('963', 'rare_fan*0', '1', '1', '1', '1.00', '#F43100,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '963', 'Festive Fan', 'As red as Rudolph\'s nose', '25');
INSERT INTO `catalogue_items` VALUES ('964', 'rare_fan*1', '1', '1', '1', '1.00', '#3C75FF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '964', 'Blue Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('965', 'rare_fan*2', '1', '1', '1', '1.00', '#55CD01,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '965', 'Green Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('966', 'rare_fan*3', '1', '1', '1', '1.00', '#BC9BFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '966', 'Purple Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('967', 'rare_fan*4', '1', '1', '1', '1.00', '#e78b8b,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '967', 'SUPERLOVE Fan', 'Fanning the fires of SUPERLOVE...', '25');
INSERT INTO `catalogue_items` VALUES ('968', 'rare_fan*5', '1', '1', '1', '1.00', '#ffcc00,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '968', 'Yellow Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('969', 'rare_fan*6', '1', '1', '1', '1.00', '#FF8000,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '969', 'Ochre Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('970', 'rare_fan*7', '1', '1', '1', '1.00', '#682B00,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '970', 'Brown Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('971', 'rare_fan*8', '1', '1', '1', '1.00', '#FFFFFF,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '971', 'Habbo Wind Turbine', 'Stylish, Eco-Energy!', '25');
INSERT INTO `catalogue_items` VALUES ('972', 'rare_fan*9', '1', '1', '1', '1.00', '#FF60B0,#FFFFFF,#FFFFFF,#FFFFFF', '0', '1', '1', '56', '972', 'Fucsia Powered Fan', 'It\'ll blow you away!', '25');
INSERT INTO `catalogue_items` VALUES ('973', 'rare_icecream*0', '1', '1', '1', '1.00', '#FFFFFF,#d02a1f', '0', '1', '1', '57', '973', 'Cherry Ice Cream', 'Virtual cherry rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('974', 'rare_icecream*1', '1', '1', '1', '1.00', '#FFFFFF,#55c4de', '0', '1', '1', '57', '974', 'Blueberry Ice Cream', 'Virtual blueberry rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('975', 'rare_icecream*2', '1', '1', '1', '1.00', '#FFFFFF,#94f718', '0', '1', '1', '57', '975', 'Pistachio Ice Cream', 'Virtual pistachio rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('976', 'rare_icecream*3', '1', '1', '1', '1.00', '#FFFFFF,#B357FF', '0', '1', '1', '57', '976', 'Blackcurrant Ice Cream', 'Virtual blackcurrant rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('977', 'rare_icecream*4', '1', '1', '1', '1.00', '#FFFFFF,#e78b8b', '0', '1', '1', '57', '977', 'Strawberry Ice Cream', 'Virtual strawberry rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('978', 'rare_icecream*5', '1', '1', '1', '1.00', '#FFFFFF,#E1CC00', '0', '1', '1', '57', '978', 'Vanilla Ice Cream', 'Virtual vanilla rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('979', 'rare_icecream*6', '1', '1', '1', '1.00', '#FFFFFF,#FF8000', '0', '1', '1', '57', '979', 'Toffee Ice Cream', 'Virtual toffee rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('980', 'rare_icecream*7', '1', '1', '1', '1.00', '#FFFFFF,#97420C', '0', '1', '1', '57', '980', 'Chocolate Ice Cream', 'Virtual chocolate rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('981', 'rare_icecream*8', '1', '1', '1', '1.00', '#FFFFFF,#00E5E2', '0', '1', '1', '57', '981', 'Peppermint Ice Cream', 'Virtual peppermint rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('982', 'rare_icecream*9', '1', '1', '1', '1.00', '#FFFFFF,#FF60B0', '0', '1', '1', '57', '982', 'Bubblegum Ice Cream', 'Virtual bubblegum rocks!', '25');
INSERT INTO `catalogue_items` VALUES ('983', 'rubberchair*1', '2', '1', '1', '1.00', '#0080FF,0,0', '0', '1', '1', '58', '983', 'Blue Inflatable Chair', 'Soft and stylish HC chair', '25');
INSERT INTO `catalogue_items` VALUES ('984', 'rubberchair*2', '2', '1', '1', '1.00', '#FF8B8B,0,0', '0', '1', '1', '58', '984', 'Pink Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('985', 'rubberchair*3', '2', '1', '1', '1.00', '#FF8000,0,0', '0', '1', '1', '58', '985', 'Orange Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('986', 'rubberchair*4', '2', '1', '1', '1.00', '#00E5E2,0,0', '0', '1', '1', '58', '986', 'Ocean Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('987', 'rubberchair*5', '2', '1', '1', '1.00', '#A1DC67,0,0', '0', '1', '1', '58', '987', 'Lime Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('988', 'rubberchair*6', '2', '1', '1', '1.00', '#B357FF,0,0', '0', '1', '1', '58', '988', 'Violet Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('989', 'rubberchair*7', '2', '1', '1', '1.00', '#CFCFCF,0,0', '0', '1', '1', '58', '989', 'White Inflatable Chair', 'Soft and tearproof!', '25');
INSERT INTO `catalogue_items` VALUES ('990', 'rubberchair*8', '2', '1', '1', '1.00', '#333333,0,0', '0', '1', '1', '58', '990', 'Black Inflatable Chair', 'Soft and tearproof for HC!', '25');
INSERT INTO `catalogue_items` VALUES ('991', 'scifiport*0', '1', '1', '1', '1.00', '#ffffff,#F43100,#ffffff,#ffffff,#ffffff,#F43100', '1', '1', '1', '59', '991', 'Red Laser Door', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('992', 'scifiport*1', '1', '1', '1', '1.00', '#ffffff,#ffbc00,#ffffff,#ffffff,#ffffff,#ffbc00', '1', '1', '1', '59', '992', 'Gold Laser Gate', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('993', 'scifiport*2', '1', '1', '1', '1.00', '#ffffff,#5599ff,#ffffff,#ffffff,#ffffff,#5599ff', '1', '1', '1', '59', '993', 'Blue Laser Gate', 'Get in the ring!', '25');
INSERT INTO `catalogue_items` VALUES ('994', 'scifiport*3', '1', '1', '1', '1.00', '#ffffff,#02bb70,#ffffff,#ffffff,#ffffff,#02bb70', '1', '1', '1', '59', '994', 'Jade Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('995', 'scifiport*4', '1', '1', '1', '1.00', '#ffffff,#e78b8b,#ffffff,#ffffff,#ffffff,#e78b8b', '1', '1', '1', '59', '995', 'Pink Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('996', 'scifiport*5', '1', '1', '1', '1.00', '#ffffff,#555555,#ffffff,#ffffff,#ffffff,#555555', '1', '1', '1', '59', '996', 'Security Fence', 'Recovered from Roswell', '25');
INSERT INTO `catalogue_items` VALUES ('997', 'scifiport*6', '1', '1', '1', '1.00', '#ffffff,#ffffff,#ffffff,#ffffff,#ffffff,#ffffff', '1', '1', '1', '59', '997', 'White Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('998', 'scifiport*7', '1', '1', '1', '1.00', '#ffffff,#00cccc,#ffffff,#ffffff,#ffffff,#00cccc', '1', '1', '1', '59', '998', 'Turquoise Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('999', 'scifiport*8', '1', '1', '1', '1.00', '#ffffff,#bb55cc,#ffffff,#ffffff,#ffffff,#bb55cc', '1', '1', '1', '59', '999', 'Purple Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_items` VALUES ('1000', 'scifiport*9', '1', '1', '1', '1.00', '#ffffff,#B357FF,#ffffff,#ffffff,#ffffff,#B357FF', '1', '1', '1', '59', '1000', 'Violet Sci-Fi Port', 'Energy beams. No trespassers!', '25');
INSERT INTO `catalogue_pages` VALUES ('1', '1', 'Frontpage', 'Frontpage', 'ctlg_frontpage2', 'catal_fp_header', 'catal_fp_pic4,catal_fp_pic5,', 'Need some Furni for your Habbo room?  Well, you?re in the right place!  This Catalogue is packed FULL of funky Furni, just click the tabs on the right to browse.<br><br>We regularly add and remove Furni, so check back regularly for new items.', 's:Need some Funky Furni for your Habbo room?', 't1:You need Habbo Credits to buy Furni for your room, click the Purse at the bottom of your screen for more information about Credits.', null);
INSERT INTO `catalogue_pages` VALUES ('5', '1', 'no rares', 'Rare', 'ctlg_norares', 'catalog_rares_headline1', 'ctlg_norare_char1,', 'There isn\'t a rare item to buy at the moment, but it\'s coming soon! Please don\'t email us about it - we\'re keeping it secret...<br>', '', '', null);
INSERT INTO `catalogue_pages` VALUES ('6', '7', 'Rare', 'Rare', 'ctlg_productpage1', 'catalog_rares_headline1', '', 'It\'s Spring, and it\'s the best time to stock up on Parasols! Go get your Green Parasol Now!:', 's:Only for 2 weeks!!!', '', null);
INSERT INTO `catalogue_pages` VALUES ('7', '1', 'Spaces', 'Spaces', 'ctlg_spaces', 'catalog_spaces_headline1', null, 'Are your walls looking a little grey?  What you need is a splash of paint and this is the place to get it!  <br><br>A splash of colour on the walls and a nice carpet can make all the difference. Use our virtual room below to test out the combinations before you buy.', 't1:Wall\rt2:Floor\rt3:Pattern\rt4:Colour\rt5:Pattern\rt6:Colour\rt7:Preview', null, null);
INSERT INTO `catalogue_pages` VALUES ('8', '1', 'Pets', 'Pets', 'ctlg_pets', 'catalog_pet_headline1', 'catalog_pets_teaser1,', 'Fluff, scales and whiskers, meows, snaps and woofs!  Anyone can have a pet on Habbo!  Select your new pet from our ever changing selection, just click to page two then click back, to see more pets!', 'u:Pets2', 'Adopt a Pet today!', null);
INSERT INTO `catalogue_pages` VALUES ('9', '1', 'Pet Accesories', 'Pet Accessories', 'ctlg_layout2', 'catalog_pet_headline2', 'ctlg_pet_teaser1,', 'You\'ll need to take care of your pet to keep it happy and healthy. This section of the Catalogue has EVERYTHING you?ll need to satisfy your pet?s needs.', 's:You\'ll have to share it!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('10', '1', 'Area', 'Area', 'ctlg_layout2', 'catalog_area_headline1', 'catalog_area_teaser1,', 'Introducing the Area Collection...  Clean, chunky lines set this collection apart as a preserve of the down-to-earth Habbo. It\'s beautiful in its simplicity, and welcoming to everyone.', 's:2: Beautiful in it\'s simplicity!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('11', '1', 'Accessories', 'Accessories', 'ctlg_layout2', 'catalog_extra_headline1', 'catalog_extra_teaser1,', 'Is your room missing something?  Well, now you can add the finishing touches that express your true personality. And don\'t forget, like everything else, these accessories can be moved about to suit your mood.', 's:2: I love my rabbit...', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('12', '1', 'Asian', 'Asian', 'ctlg_layout2', 'catalog_asian_headline1', 'catalog_asian_teaser1,', 'Dit pure haandwerk uit de eeuwenoude oosterse pixeltraditie brengt balans in elk Habbointerieur. Jin en Yang vloeien samen met Feng en Shui in een uitgebalanceerde collectie meubi. ', 's:2: Oh, look there! Is Mulan the girl of Disney xD', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('13', '1', 'Bathroom', 'Bathroom', 'ctlg_layout2', 'catalog_bath_headline1', 'catalog_bath_teaser1,', 'Introducing the Bathroom Collection...  Have some fun with the cheerful bathroom collection. Give yourself and your guests somewhere to freshen up - vital if you want to avoid nasty niffs. Put your loo in a corner though...', 's:2: Every Habbo needs one!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('14', '1', 'Candy', 'Candy', 'ctlg_layout2', 'catalog_candy_headline1', 'catalog_candy_teaser1,', 'Introducing the Candy Collection...  Candy combines the cool, clean lines of the Mode collection with a softer, more soothing style. It\'s urban sharpness with a hint of the feminine.', 's:2: Relax! It\'s faux-fur.', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('15', '1', 'Camera', 'Camera', 'ctlg_camera1', 'catalog_camera_headline1', 'campic_cam,campic_film,', 'Take a picture and keep a record of those special moments forever.  With your Habbo camera you can take pictures of just about anything in the Hotel – Even your friend on the loo...<br><br>A Camera costs 10 Credits (TWO FREE photos  included).', '', 't1:When you\'ve used your free photos, you\'ll need to buy more… Each Film (5 photos) costs 6 Credits.  <br><br>Your Camera will show how much film you ', null);
INSERT INTO `catalogue_pages` VALUES ('16', '1', 'Flags', 'Flags', 'ctlg_layout2', 'catalog_flags_headline1', 'catalog_flags_teaser1,', 'If you\'re feeling patriotic, get a flag to prove it. Our finest cloth flags will brighten up the dullest walls.', 's:2: Flag this section for later!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('17', '1', 'Gallery', 'Gallery', 'ctlg_layout2', 'catalog_gallery_headline1', 'catalog_posters_teaser1,', 'Adorn your walls with wondrous works of art, posters, plaques and wall hangings. We have items to suit all tastes, from kitsch to cool, traditional to modern.', 's:2: Brighten up your walls!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('18', '1', 'Glass', 'Glass', 'ctlg_layout2', 'catalog_glass_headline1', 'catalog_glass_teaser1,', 'Glass: Habbo Hotels exclusive furni line made from plexiglass, in different colors! Buy here your furni for a modern lounge!', 's:2: Oh My God, it\'s transparant!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('19', '1', 'Gothic', 'Gothic', 'ctlg_layout2', 'catalog_gothic_headline1', 'catalog_gothic_teaser1,', 'The Gothic section is full of medieval looking Furni. Check back for additions to this section as there are still some unreleased items to come!', 's:Gothic is my style.', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('20', '1', 'Grunge', 'Grunge', 'ctlg_layout2', 'catalog_gru_headline1', 'catalog_gru_teaser,', 'Introducing the Grunge Range. Alternative Habbos with alternative style or rebellious students with a point to prove. The Grunge range will get your bedroom looking just the way you like it - organised, neat and tidy!', '', 'New! Flaming Barrels out now!', null);
INSERT INTO `catalogue_pages` VALUES ('21', '1', 'Habbo Exchange', 'Habbo Exchange', 'ctlg_layout2', 'catalog_bank_headline1', 'catalog_bank_teaser,', 'The Habbo Exchange is where you can convert your Habbo Credits into a tradable currency. You can use this tradable currency to exchange Habbo Credits for Furni!', 's:Refundable goods!', 'Click on an item to see more details', null);
INSERT INTO `catalogue_pages` VALUES ('22', '1', 'Habbowood', 'Habbowood', 'ctlg_layout2', 'catalog_limited_headline1', 'catalog_limited_teaser1,', 'Exclusive: the new Habbowood furni, collect them all!', 's:1: Light, Camera, Action!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('23', '1', 'Iced', 'Iced', 'ctlg_layout2', 'catalog_iced_headline1', 'catalog_iced_teaser1,', 'Introducing the Iced Collection...  For the Habbo who needs no introduction. It\'s so chic, it says everything and nothing. It\'s a blank canvas, let your imagination to run wild!', 's:2: These chairs are so comfy.', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('24', '1', 'Japanese', 'Japanese', 'ctlg_layout2', 'catalog_jap_headline2', 'catalog_jap_teaser3,', 'Here you can find the new Japanese furni! Get them now!', 's:1: Brand new furni!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('25', '1', 'Lodge', 'Lodge', 'ctlg_layout2', 'catalog_lodge_headline1', 'catalog_lodge_teaser1,', 'Introducing the Lodge Collection...  Do you appreciate the beauty of wood?  For that ski lodge effect, or to match that open fire... Lodge is the Furni of choice for Habbos with that no frills approach to decorating.  <br>', 's:2: I LOVE this wood Furni!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('26', '1', 'Mode', 'Mode', 'ctlg_layout2', 'catalog_mode_headline1', 'catalog_mode_teaser1,', 'Introducing the Mode Collection...  Steely grey functionality combined with sleek designer upholstery. The Habbo that chooses this furniture is a cool urban cat - streetwise, sassy and so slightly untouchable.', 's:2: So shiny and new...', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('27', '1', 'Offers', 'Offers', 'ctlg_layout2', 'catalog_deals_headline1', 'catalog_deals_teaser1,', 'Special Offers are great if you?re just starting out. Take a look at our special collections, all at a great price.<br><br>Check them out!', '', 'Click on a deal to find out what\'s included and how much it costs.', null);
INSERT INTO `catalogue_pages` VALUES ('28', '1', 'Plants', 'Plants', 'ctlg_layout2', 'catalog_plants_headline1', 'catalog_plants_teaser1,', 'New, never before seen Bulrush Plant is here for a limited time only. Buy it now!<br>Introducing the Plant Collection...  Every room needs a plant! Not only do they bring a bit of the outside inside, they also enhance the air quality!', 's:2: I am at one with the trees', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('29', '1', 'Plastic', 'Plastic', 'ctlg_plasto', 'catalog_plasto_headline1', null, 'Introducing The Plastic Collection...  Can you feel that 1970s vibe?  Decorate with Plastic and add some colour to your life. Choose a colour that reflect your mood, or just pick your favourite shade.', 't2:Select The Colour\rs:New colours!\rt3:Preview\rt1:Choose An Item', 'Select an item and a colour and buy!', null);
INSERT INTO `catalogue_pages` VALUES ('30', '1', 'Presents', 'Presents', 'ctlg_presents', 'catalog_gifts_headline1', 'catalog_presents_teaser1,catalog_presents_teaser2,', 'Show your Habbo friends just how much you care and send them a gift from the Habbo Catalogue.  ANY Catalogue item can be sent as a gift to ANY Habbo, all you need is their Habbo name!', 't1:Buying an item as a gift couldn?to be simpler...  <br><br>Buy an item from the Catalogue in the normal way, but tick \'buy as a gift\'. Tell us which Habbo you want to give the gift to and we\'ll gift wrap it and deliver it straight to their hand.', '', null);
INSERT INTO `catalogue_pages` VALUES ('31', '1', 'Pura', 'Pura', 'ctlg_layout2', 'catalog_pura_headline1', 'catalog_pura_teaser1,', 'Introducing the Pura Collection...  This collection breathes fresh, clean air and cool tranquility. The Pura Waltzer has arrived!  Check back regularly to see  new colours of Pura on sale!', '', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('32', '1', 'Recycler', 'Recycler', 'ctlg_recycler', 'catalog_recycler_headline1', null, null, '', null, null);
INSERT INTO `catalogue_pages` VALUES ('33', '1', 'Rollers', 'Rollers', 'ctlg_layout2', 'catalog_roller_headline1', '', 'Move your imagination, while you move your Habbo!  Perfect for mazes, games, for keeping your queue moving or making your pet go round in circles for hours.  Available in multi-packs ? the more you buy the cheaper the Roller! Pink Rollers out now!', 's:You can fit 35 Rollers in a Guest Room!', 'Click on a Roller to see more information!', null);
INSERT INTO `catalogue_pages` VALUES ('34', '1', 'Romantique', 'Romantique', 'ctlg_layout2', 'catalog_romantique_headline1', 'catalog_rom_teaser,', 'The Romantique range features Grand Pianos, old antique lamps and tables. It is the ideal range for setting a warm and loving mood in your room. Spruce up your room and invite that special someone over. Now featuring the extra special COLOUR edition.', '', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('35', '1', 'Rugs', 'Rugs', 'ctlg_layout2', 'catalog_rugs_headline1', 'catalog_rugs_teaser1,', 'We have rugs for all occasions.  All rugs are non-slip and washable.', 's:2: Rugs, rugs and more rugs!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('36', '1', 'Sports', 'Sports', 'ctlg_layout2', 'catalog_sports_headline1', 'catalog_sports_teaser1,', 'For the sporty habbos, here is the sports section!', null, 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('37', '1', 'Teleport', 'Teleport', 'ctlg_layout2', 'catalog_doors_headline1', 'catalog_teleports_teaser2,catalog_door_c,catalog_door_b,', 'Beam your Habbo from one room to another with one of our cunningly disguised, space age teleports. Now you can link any two rooms together! Teleports are sold in pairs, so if you trade for them, check you\'re getting a linked pair.', 's:Teleport!', 'Select an item by clicking one of the icons on the left.', null);
INSERT INTO `catalogue_pages` VALUES ('38', '1', 'Trax Ambient', 'Trax Ambient', 'ctlg_soundmachine', 'catalog_trx_header1', 'catalog_trx_teaser1,', 'Welcome to the Ambient Trax Store! With groovy beats and chilled out melodies, this is the section for some cool and relaxing tunes.', null, null, null);
INSERT INTO `catalogue_pages` VALUES ('39', '1', 'Trax Dance', 'Trax Dance', 'ctlg_soundmachine', 'catalog_trx_header2', 'catalog_trx_teaser2,', 'Welcome to the Dance Trax Store! With funky beats and catchy melodies, this is the section for every clubber  to indulge in.', null, null, null);
INSERT INTO `catalogue_pages` VALUES ('40', '1', 'Trax Rock', 'Trax Rock', 'ctlg_soundmachine', 'catalog_trx_header3', 'catalog_trx_teaser3,', 'Welcome to the Rock Trax Store! With heavy beats and rockin\' riffs, this is the section for every rock fan to experiment with.', null, null, null);
INSERT INTO `catalogue_pages` VALUES ('41', '1', 'Trax SFX', 'Trax SFX', 'ctlg_soundmachine', 'catalog_trx_header4', 'catalog_trx_teaser4,', 'Welcome to the SFX Trax Store! With crazy sounds and weird noises, this is the section for every creative room builder  to indulge in.', null, null, null);
INSERT INTO `catalogue_pages` VALUES ('42', '1', 'Trax Urban', 'Trax Urban', 'ctlg_soundmachine', 'catalog_trx_header5', 'catalog_trx_teaser5,', 'Welcome to the Urban Trax Store! With hip hop beats and RnB vocals, this is the section for every city bopper  to indulge in.', null, null, null);
INSERT INTO `catalogue_pages` VALUES ('43', '1', 'Trophies', 'Trophies', 'ctlg_trophies', 'catalog_trophies_headline1', '', 'Reward your Habbo friends, or yourself with one of our fabulous glittering array of bronze, silver and gold trophies.<br><br>First choose the trophy model (click on the arrows to see all the different styles) and then the metal (click on the seal below the trophy). Type your inscription below and we\'ll engrave it on the trophy along with your name and today\'s date.', 't1:Type your inscription CAREFULLY, it\'s permanent!', null, null);
INSERT INTO `catalogue_pages` VALUES ('44', '7', 'Christmas', 'Christmas', 'ctlg_layout2', 'catalog_xmas_headline1', 'catalog_xmas_teaser,', 'The Habbo Christmas catalogue has everything you need to make the perfect festive room: Trees, ducks with santa hats on, puddings and of course, Menorahs!', 's:2:Tuck into Christmas!', 'Click on an item to see a bigger version of it!', null);
INSERT INTO `catalogue_pages` VALUES ('45', '7', 'Easter', 'Easter', 'ctlg_layout2', 'catalog_easter_headline1', 'catalog_easter_teaser1,', '\'Egg\'cellent furni - Bouncing bunnies, fluffy chicks, choccy eggs... Yep, it\'s Easter! Celebrate with something \'eggs\'tra special from our Easter range. But hurry - it\'s not here for long this year!', 's:2: \'Egg\'-Tastic!', 'Click on an item for more details.', null);
INSERT INTO `catalogue_pages` VALUES ('46', '7', 'Halloween', 'Halloween', 'ctlg_layout2', 'catalog_halloween_headline1', 'catalog_halloween_teaser,', 'Yes, it\'s a spookfest! Get your boney hands on our creepy collection of ghoulish goodies. But be quick - they\'ll be gone from the Catalogue after two weeks!', 's:2:Halloween is My day!', 'Click on an item to see a bigger version of it!', null);
INSERT INTO `catalogue_pages` VALUES ('47', '7', 'Love', 'Love', 'ctlg_layout2', 'catalog_love_headline1', 'catalog_love_teaser1,', 'The love collection has everything to create the perfect love room, for a good price!', 's:2:Oh! Comes Valentine\'s Day!', 'Click on an item to see a bigger version of it!', null);
INSERT INTO `catalogue_pages` VALUES ('48', '7', 'Area Colour', 'Area Colour', 'ctlg_layout2', 'catalog_area_headline1', 'catalog_area_teaser1,', 'Introducing the Area Collection...  Clean, chunky lines set this collection apart as a preserve of the down-to-earth Habbo. It\'s beautiful in its simplicity, and welcoming to everyone.', 's:Much More Colours!!!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('49', '7', 'Iced Colour', 'Iced Colour', 'ctlg_layout2', 'catalog_iced_headline1', 'catalog_iced_teaser1,', 'Introducing the Iced Collection...  For the Habbo who needs no introduction. It\'s so chic, it says everything and nothing. It\'s a blank canvas, let your imagination to run wild!', 's:Much More Colours!!!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('50', '7', 'Mode Colour', 'Mode Colour', 'ctlg_layout2', 'catalog_mode_headline1', 'catalog_mode_teaser1,', 'Introducing the Mode Collection...  Steely grey functionality combined with sleek designer upholstery. The Habbo that chooses this furniture is a cool urban cat - streetwise, sassy and so slightly untouchable.', 's:Much More Colours!!!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('51', '7', 'Pura Colour', 'Pura Colour', 'ctlg_layout2', 'catalog_pura_headline1', 'catalog_pura_teaser1,', 'Introducing the Pura Collection...  This collection breathes fresh, clean air and cool tranquility. The Pura Waltzer has arrived!  Check back regularly to see  new colours of Pura on sale!', 's:Much More Colours!!!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('52', '7', 'Romantique Colour', 'Romantique Colour', 'ctlg_layout2', 'catalog_romantique_headline1', 'catalog_rom_teaser,', 'The Romantique range features Grand Pianos, old antique lamps and tables. It is the ideal range for setting a warm and loving mood in your room. Spruce up your room and invite that special someone over. Now featuring the extra special COLOUR edition.', 's:Much More Colours!!!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('53', '7', 'Club Furni', 'Club Furni', 'ctlg_layout2', 'catalog_club_headline1', 'catalog_hc_teaser,', 'Welcome to the club furni page. Here you can buy any club furni for just 25 credits!', 's:For Habbo Club members only!', 'Click on an item to see more details', null);
INSERT INTO `catalogue_pages` VALUES ('54', '7', 'Club Shop', 'Club Shop', 'ctlg_layout2', 'catalog_club_headline1', 'catalog_hc_teaser,', 'Welcome to the Brand New Habbo Club Shop, where Habbo Club members can purchase exclusive items!<br>The Furni in this section can only be purchased by Habbos who have joined Habbo Club.', 's:For Habbo Club members only!', 'Click on an item to see more details', null);
INSERT INTO `catalogue_pages` VALUES ('55', '7', 'Dragons', 'Dragons', 'ctlg_layout2', 'catalog_jap_headline2', 'catalog_jap_teaser3,', 'Here you can find the Power of Dragons! Get them now!', 's:1: All Colours!', 'Click on the item you want for more information.', null);
INSERT INTO `catalogue_pages` VALUES ('56', '7', 'Fans', 'Fans', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('57', '7', 'Ice Cream Rare', 'Ice Cream Rare', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('58', '7', 'Inflatables', 'Inflatables', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('59', '7', 'Laser Gates', 'Laser Gates', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('60', '7', 'Marquee', 'Marquee', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('61', '7', 'Monoliths', 'Monoliths', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('62', '7', 'Oriental Screen', 'Oriental Screen', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('63', '7', 'Pilars Rare', 'Pilars Rare', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('64', '7', 'Pillow Rare', 'Pillow Rare', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('65', '7', 'Smoke Machines', 'Smoke Machines', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('66', '7', 'Summer Pools', 'Summer Pools', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('67', '7', 'Rares', 'Rares', 'ctlg_layout2', 'catalog_rares_headline1', '', 'Hm... You have permission to stay here? Oh! if you don\'t have permission, please go away!!!', 's:Restricted Zone', 'Select a item, and see more details', null);
INSERT INTO `catalogue_pages` VALUES ('68', '7', 'Trophies Rare', 'Tropies Rare', '', '', '', '', '', '', null);
INSERT INTO `catalogue_pages` VALUES ('69', '7', 'Limited Edition', 'Limited Edition', 'ctlg_layout2', 'catalog_limited_headline1', 'catalog_limited_teaser1,', 'Get it while its hot.. <br><br>LIMITED EDITION FURNITURE!', '', 'For a limited time only!', null);
INSERT INTO `cms_news` VALUES ('1', 'Installation Complete', 'HoloCMS', 'http://images.habbohotel.co.uk/c_images/Top_Story_Images/topstory_accessories2.gif', 'HoloCMS was successfully installed!', 'Thank you for choosing HoloCMS! Everything appears to be working just fine..\r\n\r\nYou can edit this article by going into housekeeping and choosing Manage Existing Articles.', '2008-03-22', 'Meth0d');
INSERT INTO `cms_news` VALUES ('1', 'Installation Complete', 'HoloCMS', 'http://images.habbohotel.co.uk/c_images/Top_Story_Images/topstory_accessories2.gif', 'HoloCMS was successfully installed!', 'Thank you for choosing HoloCMS! Everything appears to be working just fine..\r\n\r\nYou can edit this article by going into housekeeping and choosing Manage Existing Articles.', '2008-03-22', 'Meth0d');
INSERT INTO `cms_system` VALUES ('Holo Hotel', 'Holo', '0', '1', 'en', '127.0.0.1', '21', 'http://www.habbohotel.co.uk/gamedata/external?id=external_texts', 'http://www.habbohotel.co.uk/gamedata/external?id=external_variables', 'http://images.habbohotel.co.uk/dcr/r21_20080317_0342_4666_5527e6590eba8f3fb66348bdf271b5a2/habbo.dcr', 'http://www.mysite.com/cms/client.php', '0');
INSERT INTO `cms_tags` VALUES ('1', 'Nillus', 'butt');
INSERT INTO `cms_tags` VALUES ('2', 'NGangsta', 'rosa');
INSERT INTO `cms_tags` VALUES ('3', 'Nillus', 'kewl');
INSERT INTO `cms_tags` VALUES ('4', 'Nillus', 'shiz');
INSERT INTO `cms_tags` VALUES ('5', 'Nillus', 'begginyouformercy');
INSERT INTO `cms_tags` VALUES ('6', 'Nillus', 'hamster');
INSERT INTO `cms_tags` VALUES ('7', 'Nillus', 'ngangsta');
INSERT INTO `cms_tags` VALUES ('8', 'Nillus', 'aaronisafag');
INSERT INTO `guestroom_modeldata` VALUES ('a', '3,5,0', 'xxxxxxxxxxxx\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxx00000000\rxxxxxxxxxxxx\rxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('b', '0,5,0', 'xxxxxxxxxxxx\rxxxxx0000000\rxxxxx0000000\rxxxxx0000000\rxxxxx0000000\rx00000000000\rx00000000000\rx00000000000\rx00000000000\rx00000000000\rx00000000000\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('c', '4,7,0', 'xxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('d', '4,7,0', 'xxxxxxxxxxxx\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxx000000x\rxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('e', '1,5,0', 'xxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxx0000000000\rxx0000000000\rxx0000000000\rxx0000000000\rxx0000000000\rxx0000000000\rxx0000000000\rxx0000000000\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('f', '2,5,0', 'xxxxxxxxxxxx\rxxxxxxx0000x\rxxxxxxx0000x\rxxx00000000x\rxxx00000000x\rxxx00000000x\rxxx00000000x\rx0000000000x\rx0000000000x\rx0000000000x\rx0000000000x\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('g', '1,7,1', 'xxxxxxxxxxxxx\rxxxxxxxxxxxxx\rxxxxxxx00000x\rxxxxxxx00000x\rxxxxxxx00000x\rxx1111000000x\rxx1111000000x\rxx1111000000x\rxx1111000000x\rxx1111000000x\rxxxxxxx00000x\rxxxxxxx00000x\rxxxxxxx00000x\rxxxxxxxxxxxxx\rxxxxxxxxxxxxx\rxxxxxxxxxxxxx\rxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('h', '4,4,1', 'xxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxx111111x\rxxxxx111111x\rxxxxx111111x\rxxxxx111111x\rxxxxx111111x\rxxxxx000000x\rxxxxx000000x\rxxx00000000x\rxxx00000000x\rxxx00000000x\rxxx00000000x\rxxxxxxxxxxxx\rxxxxxxxxxxxx\rxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('i', '0,10,0', 'xxxxxxxxxxxxxxxxx\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rx0000000000000000\rxxxxxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('j', '0,10,0', 'xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx0000000000\rxxxxxxxxxxx0000000000\rxxxxxxxxxxx0000000000\rxxxxxxxxxxx0000000000\rxxxxxxxxxxx0000000000\rxxxxxxxxxxx0000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx0000000000xxxxxxxxxx\rx0000000000xxxxxxxxxx\rx0000000000xxxxxxxxxx\rx0000000000xxxxxxxxxx\rx0000000000xxxxxxxxxx\rx0000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('k', '0,13,0', 'xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx00000000\rxxxxxxxxxxxxxxxxx00000000\rxxxxxxxxxxxxxxxxx00000000\rxxxxxxxxxxxxxxxxx00000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rx000000000000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxxxxxxxxxxxxxxxxxx\r\r');
INSERT INTO `guestroom_modeldata` VALUES ('l', '0,16,0', 'xxxxxxxxxxxxxxxxxxxxx\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rx00000000xxxx00000000\rxxxxxxxxxxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('m', '0,15,0', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rx0000000000000000000000000000\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxx00000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('n', '0,16,0', 'xxxxxxxxxxxxxxxxxxxxx\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx000000xxxxxxxx000000\rx000000x000000x000000\rx000000x000000x000000\rx000000x000000x000000\rx000000x000000x000000\rx000000x000000x000000\rx000000x000000x000000\rx000000xxxxxxxx000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rx00000000000000000000\rxxxxxxxxxxxxxxxxxxxxx\r');
INSERT INTO `guestroom_modeldata` VALUES ('o', '0,18,1', 'xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx11111111xxxx\rxxxxxxxxxxxxx00000000xxxx\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rx111111100000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxx0000000000000000\rxxxxxxxxxxxxxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('p', '0,23,2', 'xxxxxxxxxxxxxxxxxxx\rxxxxxxx222222222222\rxxxxxxx222222222222\rxxxxxxx222222222222\rxxxxxxx222222222222\rxxxxxxx222222222222\rxxxxxxx222222222222\rxxxxxxx22222222xxxx\rxxxxxxx11111111xxxx\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx222221111111111111\rx2222xx11111111xxxx\rx2222xx00000000xxxx\rx2222xx000000000000\rx2222xx000000000000\rx2222xx000000000000\rx2222xx000000000000\rx2222xx000000000000\rx2222xx000000000000\rxxxxxxxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('q', '10,4,2', 'xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx22222222\rxxxxxxxxxxx22222222\rxxxxxxxxxxx22222222\rxxxxxxxxxxx22222222\rxxxxxxxxxxx22222222\rxxxxxxxxxxx22222222\rx222222222222222222\rx222222222222222222\rx222222222222222222\rx222222222222222222\rx222222222222222222\rx222222222222222222\rx2222xxxxxxxxxxxxxx\rx2222xxxxxxxxxxxxxx\rx2222211111xx000000\rx222221111110000000\rx222221111110000000\rx2222211111xx000000\rxx22xxx1111xxxxxxxx\rxx11xxx1111xxxxxxxx\rx1111xx1111xx000000\rx1111xx111110000000\rx1111xx111110000000\rx1111xx1111xx000000\rxxxxxxxxxxxxxxxxxxx');
INSERT INTO `guestroom_modeldata` VALUES ('r', '10,4,3', 'xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx33333333333333\rxxxxxxxxxxx33333333333333\rxxxxxxxxxxx33333333333333\rxxxxxxxxxxx33333333333333\rxxxxxxxxxxx33333333333333\rxxxxxxxxxxx33333333333333\rxxxxxxx333333333333333333\rxxxxxxx333333333333333333\rxxxxxxx333333333333333333\rxxxxxxx333333333333333333\rxxxxxxx333333333333333333\rxxxxxxx333333333333333333\rx4444433333xxxxxxxxxxxxxx\rx4444433333xxxxxxxxxxxxxx\rx44444333333222xx000000xx\rx44444333333222xx000000xx\rxxx44xxxxxxxx22xx000000xx\rxxx33xxxxxxxx11xx000000xx\rxxx33322222211110000000xx\rxxx33322222211110000000xx\rxxxxxxxxxxxxxxxxx000000xx\rxxxxxxxxxxxxxxxxx000000xx\rxxxxxxxxxxxxxxxxx000000xx\rxxxxxxxxxxxxxxxxx000000xx\rxxxxxxxxxxxxxxxxxxxxxxxxx');
INSERT INTO `nav_categories` VALUES ('0', '1', 'No category', '0', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('3', '0', 'Public Rooms', '1', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('4', '0', 'Guestrooms', '0', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('5', '3', 'Aaron is a fag - free beef here', '1', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('6', '5', 'Get ya beef here', '1', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('7', '6', 'Do you smell it?', '1', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('8', '7', 'Stop sniffing!', '1', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('9', '4', 'Staff rooms', '0', '6', '0', '0');
INSERT INTO `nav_categories` VALUES ('10', '9', 'Staff-only rooms', '0', '6', '1', '1');
INSERT INTO `nav_categories` VALUES ('11', '4', 'Trading rooms', '0', '1', '0', '1');
INSERT INTO `nav_categories` VALUES ('12', '4', 'Rape floor', '0', '1', '0', '0');
INSERT INTO `nav_categories` VALUES ('13', '4', 'Disco floor', '0', '1', '0', '0');
INSERT INTO `publicrooms` VALUES ('14', '3', 'Welcome Lounge', 'hh_room_nlobby', 'welcome_lounge', 'newbie_lobby', '0', '35', 'xxxxxxxxxxxxxxxx000000\rxxxxx0xxxxxxxxxx000000\rxxxxx00000000xxx000000\rxxxxx000000000xx000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\rxxxxx000000000000000xx\rxxxxx000000000000000xx\rx0000000000000000000xx\rx0000000000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxxx0000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxx000000000000xxxxx\rxxxxx000000000000xxxxx\r', 'a160 crl_lamp 16 0 0 0 1\r\ny170 crl_sofa2c 17 0 0 4 2\r\nw180 crl_sofa2b 18 0 0 4 2\r\nv190 crl_sofa2a 19 0 0 4 2\r\na200 crl_lamp 20 0 0 0 1\r\nb161 crl_chair 16 1 0 2 2\r\na72 crl_lamp 7 2 0 0 1\r\na112 crl_lamp 11 2 0 0 1\r\nb162 crl_chair 16 2 0 2 2\r\nc53 crl_pillar 5 3 0 0 1\r\nb73 crl_chair 7 3 0 2 2\r\nu93 crl_table1b 9 3 0 0 1\r\ns113 crl_sofa1c 11 3 0 6 2\r\nb163 crl_chair 16 3 0 2 2\r\nA193 crl_table2b 19 3 0 0 1\r\nz203 crl_table2a 20 3 0 0 1\r\na04 crl_lamp 0 4 0 0 1\r\ny14 crl_sofa2c 1 4 0 4 2\r\nw24 crl_sofa2b 2 4 0 4 2\r\nv34 crl_sofa2a 3 4 0 4 2\r\na44 crl_lamp 4 4 0 0 1\r\nt94 crl_table1a 9 4 0 0 1\r\nr114 crl_sofa1b 11 4 0 6 2\r\nh154 crl_wall2a 15 4 0 0 1\r\na164 crl_lamp 16 4 0 0 1\r\nb05 crl_chair 0 5 0 2 2\r\nb75 crl_chair 7 5 0 2 2\r\nq115 crl_sofa1a 11 5 0 6 2\r\nA26 crl_table2b 2 6 0 0 1\r\nz36 crl_table2a 3 6 0 0 1\r\na116 crl_lamp 11 6 0 0 1\r\nb07 crl_chair 0 7 0 2 2\r\na08 crl_lamp 0 8 0 0 1\r\nD18 crl_sofa3c 1 8 0 0 2\r\nC28 crl_sofa3b 2 8 0 0 2\r\nB38 crl_sofa3a 3 8 0 0 2\r\na48 crl_lamp 4 8 0 0 1\r\no198 crl_barchair2 19 8 0 0 2\r\np208 crl_tablebar 20 8 0 0 1\r\no218 crl_barchair2 21 8 0 0 2\r\nE59 crl_pillar2 5 9 0 0 1\r\nc99 crl_pillar 9 9 0 0 1\r\nP815 crl_desk1a 8 15 0 0 1\r\nO915 crl_deski 9 15 0 0 1\r\nN1015 crl_deskh 10 15 0 0 1\r\nM1016 crl_deskg 10 16 0 0 1\r\nL1017 crl_deskf 10 17 0 0 1\r\nK1018 crl_deske 10 18 0 0 1\r\nK1019 crl_deske 10 19 0 0 1\r\nK1020 crl_deske 10 20 0 0 1\r\nK1021 crl_deske 10 21 0 0 1\r\nK1022 crl_deske 10 22 0 0 1\r\nK1023 crl_deske 10 23 0 0 1\r\nG724 crl_wallb 7 24 0 0 1\r\nK1024 crl_deske 10 24 0 0 1\r\nF725 crl_walla 7 25 0 0 1\r\nH825 crl_desk1b 8 25 0 0 1\r\nI925 crl_desk1c 9 25 0 0 1\r\nJ1025 crl_desk1d 10 25 0 0 1\r\nd1227 crl_lamp2 12 27 0 0 1\r\nf1327 crl_cabinet2 13 27 0 0 1\r\ne1427 crl_cabinet1 14 27 0 0 1\r\nd1527 crl_lamp2 15 27 0 0 1', '2 11 0');
INSERT INTO `publicrooms` VALUES ('15', '3', 'The Lido', 'hh_room_pool,hh_people_pool', 'habbo_lido', 'pool_a', '0', '70', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx7xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx777xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx7777777xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx777777777xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx777777xxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7x777777777xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx77777777xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7x777777777x7xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx7777777777xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx777777777777xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777777x2111xxxxxxxxxxxx\rxxxxxxxxxxxxxxx7777777777x221111xxxxxxxxxxx\rxxxxxxxxx7777777777777777x2211111xxxxxxxxxx\rxxxxxxxxx7777777777777777x22211111xxxxxxxxx\rxxxxxxxxx7777777777777777x222211111xxxxxxxx\rxxxxxx77777777777777777777x222211111xxxxxxx\rxxxxxx7777777xx777777777777x222211111xxxxxx\rxxxxxx7777777xx77777777777772222111111xxxxx\rxxxxxx777777777777777777777x22221111111xxxx\rxx7777777777777777777777x322222211111111xxx\r77777777777777777777777x33222222111111111xx\r7777777777777777777777x333222222211111111xx\rxx7777777777777777777x333322222222111111xxx\rxx7777777777777777777333332222222221111xxxx\rxx777xxx777777777777733333222222222211xxxxx\rxx777x7x77777777777773333322222222222xxxxxx\rxx777x7x7777777777777x33332222222222xxxxxxx\rxxx77x7x7777777777777xx333222222222xxxxxxxx\rxxxx77777777777777777xxx3222222222xxxxxxxxx\rxxxxx777777777777777777xx22222222xxxxxxxxxx\rxxxxxx777777777777777777x2222222xxxxxxxxxxx\rxxxxxxx777777777777777777222222xxxxxxxxxxxx\rxxxxxxxx7777777777777777722222xxxxxxxxxxxxx\rxxxxxxxxx77777777777777772222xxxxxxxxxxxxxx\rxxxxxxxxxx777777777777777222xxxxxxxxxxxxxxx\rxxxxxxxxxxx77777777777777x2xxxxxxxxxxxxxxxx\rxxxxxxxxxxxx77777777777777xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx777777777777xxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx7777777777xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx777777xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx7777xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx77xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'u184 umbrella2 18 4 7 0 1\r\nb185 pool_2sofa2 18 5 7 2 2\r\nB186 pool_2sofa1 18 6 7 2 2\r\nu720 umbrella2 7 20 7 0 1\r\nc820 pool_chair2 8 20 7 4 2\r\nu2420 umbrella2 24 20 7 0 1\r\nc2520 pool_chair2 25 20 7 4 2\r\nc721 pool_chair2 7 21 7 2 2\r\nt821 pool_table2 8 21 7 0 2\r\nc921 pool_chair2 9 21 7 6 2\r\nc2421 pool_chair2 24 21 7 2 2\r\nt2521 pool_table2 25 21 7 0 1\r\nc822 pool_chair2 8 22 7 0 2\r\nR228 flower2b 2 28 7 0 1\r\nr229 flower2a 2 29 7 0 1\r\nL632 box 6 32 7 0 1\r\nf1333 flower1 13 33 7 0 1\r\ny1034 pool_chairy 10 34 7 4 2\r\n8835 umbrellay 8 35 7 0 1\r\ny935 pool_chairy 9 35 7 2 2 2\r\nQ1035 pool_tabley 10 35 7 0 1\r\ny1135 pool_chairy 11 35 7 6 2\r\n91535 umbrellap 15 35 7 0 1\r\n72135 umbrellao 21 35 7 0 1\r\ny1036 pool_chairy 10 36 7 0 2\r\nP1536 pool_chairp 15 36 7 4 2\r\no2136 pool_chairo 21 36 7 4 2\r\no2236 pool_chairo 22 36 7 4 2\r\nP1437 pool_chairp 14 37 7 2 2\r\nZ1537 pool_tablep 15 37 7 0 1\r\nP1637 pool_chairp 16 37 7 6 2\r\nK2137 pool_tabo 21 37 7 6 1\r\nk2237 pool_tabo2 22 37 7 6 1\r\nP1438 pool_chairp 14 38 7 2 2\r\nz1538 pool_tablep2 15 38 7 0 1\r\nP1638 pool_chairp 16 38 7 6 2\r\no2138 pool_chairo 21 38 7 0 2\r\no2238 pool_chairo 22 38 7 0 2\r\nP1539 pool_chairp 15 39 7 0 2\r\ng2041 pool_chairg 20 41 7 4 2\r\ng2141 pool_chairg 21 41 7 4 2\r\nW2042 pool_tablg 20 42 7 6 1\r\nw2142 pool_tablg2 21 42 7 6 1\r\nG1943 umbrellag 19 43 7 0 1\r\ng2043 pool_chairg 20 43 7 0 2\r\ng2143 pool_chairg 21 43 7 0 2', '2 25 7');
INSERT INTO `publicrooms` VALUES ('16', '3', 'Holo - Oldskool', 'hh_room_old_skool', 'old_skool', 'old_skool1', '0', '20', 'x6666666665432100\rx6666666665432100\rx6600000000000x00\rx6600000000000000\rx6600000000000000\rx6600000000000000\rx660000000000x000\r666000000000x1111\rx66000000000xx111\rx66000000000x1111\rx66000000000x1111\rx55000000000x1111\rx44000000000x1111\rx33000000000x1111\rx22000000000xx111\rx11x00000000x1111\rx00000000000x1111\rx00000000000xx111', 'j42 mobiles_chair2 4 2 0 4 2\r\nj92 mobiles_chair2 9 2 0 4 2\r\ng43 mobiles_table6 4 3 0 0 1\r\nh53 mobiles_table7 5 3 0 0 1\r\nj73 mobiles_chair2 7 3 0 2 2\r\ng83 mobiles_table6 8 3 0 0 1\r\nh93 mobiles_table7 9 3 0 0 1\r\nj34 mobiles_chair2 3 4 0 2 2\r\nf44 mobiles_table5 4 4 0 0 1\r\ni54 mobiles_table8 5 4 0 0 1\r\nf84 mobiles_table5 8 4 0 0 1\r\ni94 mobiles_table8 9 4 0 0 1\r\nj85 mobiles_chair2 8 5 0 0 2\r\nj56 mobiles_chair2 5 6 0 4 2\r\nj37 mobiles_chair2 3 7 0 2 2\r\ng47 mobiles_table6 4 7 0 0 1\r\nh57 mobiles_table7 5 7 0 0 1\r\nf48 mobiles_table5 4 8 0 0 1\r\ni58 mobiles_table8 5 8 0 0 1\r\nj49 mobiles_chair2 4 9 0 0 2', '1 7 6');
INSERT INTO `publicrooms` VALUES ('17', '3', 'SnowStorm lobby', 'hh_gamesys,hh_game_snowwar,hh_game_snowwar_room,hh_game_snowwar_ui', 'sw_lobby_amateur_6', 'snowwar_lobby_1', '0', '10', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx11111xx1xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx11111xx1111xxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx1111x1111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'a3118 sw_barrellchair 31 18 1 6 2\r\na3119 sw_barrellchair 31 19 1 6 2\r\na3020 sw_barrellchair 30 20 1 0 2\r\na3720 sw_barrellchair 37 20 1 0 2\r\na3920 sw_barrellchair 39 20 1 0 2\r\na4120 sw_barrellchair 41 20 1 0 2\r\nd3024 sw_chair1 30 24 1 4 2\r\nc3124 sw_chair2 31 24 1 4 2\r\nc3224 sw_chair2 32 24 1 4 2\r\nc3324 sw_chair2 33 24 1 4 2\r\nb3424 sw_chair3 34 24 1 4 2\r\ni3025 sw_table1 30 25 1 0 1\r\nh3125 sw_table2 31 25 1 0 1\r\ng3225 sw_table3 32 25 1 0 1\r\nf3325 sw_table4 33 25 1 0 1\r\ne3425 sw_table5 34 25 1 0 1\r\nd3026 sw_chair1 30 26 1 0 2\r\nc3126 sw_chair2 31 26 1 0 2\r\nc3226 sw_chair2 32 26 1 0 2\r\nc3326 sw_chair2 33 26 1 0 2\r\nb3426 sw_chair3 34 26 1 0 2\r\nd3029 sw_chair1 30 29 1 4 2\r\nc3129 sw_chair2 31 29 1 4 2\r\nc3229 sw_chair2 32 29 1 4 2\r\nc3329 sw_chair2 33 29 1 4 2\r\nb3429 sw_chair3 34 29 1 4 2\r\ni3030 sw_table1 30 30 1 0 1\r\nh3130 sw_table2 31 30 1 0 1\r\ng3230 sw_table3 32 30 1 0 1\r\nf3330 sw_table4 33 30 1 0 1\r\ne3430 sw_table5 34 30 1 0 1\r\nd3031 sw_chair1 30 31 1 0 2\r\nc3131 sw_chair2 31 31 1 0 2\r\nc3231 sw_chair2 32 31 1 0 2\r\nc3331 sw_chair2 33 31 1 0 2\r\nb3431 sw_chair3 34 31 1 0 2\r\nx2732 invisichair 27 32 1 2 2\r\nx2733 invisichair 27 33 1 2 2\r\nx2734 invisichair 27 34 1 2 2\r\nx2836 invisichair 28 36 1 0 2\r\nx2936 invisichair 29 36 1 0 2\r\nx3036 invisichair 30 36 1 0 2\r\nx3136 invisichair 31 36 1 0 2', '41 36 1');
INSERT INTO `publicrooms` VALUES ('91', '0', 'Sky Peak', null, null, 'bb_arena_1', '0', '30', 'xxxxxxxxxxxxx444444xxxxxxxxx\rxxxxxxxxxxxxx444444xxxxxxxxx\rxxxxxxxxxxxxx444444xxxxxxxxx\rxxx22222222xx444444xxxxxxxxx\rxxx22222222xx444444xxxxxxxxx\rxxx22222222xx333333xxxxxxxxx\rxxx22222222xx222222xxxxxxxxx\rxxx222222222222222222xxxxxxx\rxxx222222222222222222xxxxxxx\rxxx2222222222222222222100000\rxxx2222222222222222222100000\rxxxxxxx222222222222222100000\rxxxxxxx222222222222222100000\r4444432222222222222222100000\r4444432222222222222222100000\r444443222222222222222xxxxxxx\r444443222222222222222xxxxxxx\r4444432222222222222222222xxx\r4444432222222222222222222xxx\rxxxxxxx222222222222222222xxx\rxxxxxxx222222222222222222xxx\rxxxxxxxxx222222xx22222222xxx\rxxxxxxxxx111111xx22222222xxx\rxxxxxxxxx000000xx22222222xxx\rxxxxxxxxx000000xx22222222xxx\rxxxxxxxxx000000xxxxxxxxxxxxx\rxxxxxxxxx000000xxxxxxxxxxxxx\rxxxxxxxxx000000xxxxxxxxxxxxx\r', null, null);
INSERT INTO `publicrooms` VALUES ('92', '0', 'Coral Beach', null, null, 'bb_arena_2', '0', '30', null, null, null);
INSERT INTO `publicrooms` VALUES ('93', '0', 'Maze Park', null, null, 'bb_arena_3', '0', '30', null, null, null);
INSERT INTO `publicrooms` VALUES ('94', '0', 'Gothic Hallway', null, null, 'bb_arena_4', '0', '30', null, null, null);
INSERT INTO `publicrooms` VALUES ('95', '0', 'Barebones Classic', null, null, 'bb_arena_5', '0', '30', null, null, null);
INSERT INTO `publicrooms` VALUES ('18', '8', 'BattleBall lobby', 'hh_game_bb,hh_game_bb_room,hh_game_bb_ui,hh_gamesys', 'bb_lobby_beginner_2', 'bb_lobby_1', '0', '30', 'xxx2222222222222222x\rxxx2222222222222222x\rxxx2222222222222222x\rxxx2222222222222222x\rxxx11111111111111111\r11x11111111111111111\r11x11111111111111111\r11x11111111111111111\rx1x11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxxxxxx000xxxx\rxxxxxxxxxxxxx000xxxx\rxxxxxxxxxxxxx000xxxx\r', 'n30 bb_crossrd 3 0 2 2 1\r\nb40 bb_bench1 4 0 2 4 2\r\nc50 bb_bench2 5 0 2 4 2\r\nh80 bb_plant1 8 0 2 0 1\r\nd90 bb_sofa1 9 0 2 4 2\r\ne100 bb_sofa2 10 0 2 4 2\r\nh110 bb_plant1 11 0 2 0 1\r\nd120 bb_sofa1 12 0 2 4 2\r\ne130 bb_sofa2 13 0 2 4 2\r\nh140 bb_plant1 14 0 2 0 1\r\nb160 bb_bench1 16 0 2 4 2\r\nc170 bb_bench2 17 0 2 4 2\r\nk180 bb_corner1out 18 0 2 0 1\r\nb31 bb_bench1 3 1 2 2 2\r\no181 bb_wallend1in 18 1 2 2 1\r\nc32 bb_bench2 3 2 2 2 2\r\no182 bb_wallend1in 18 2 2 2 1\r\nj33 bb_plant3 3 3 2 0 1\r\nt73 bb_special 7 3 2 6 1\r\no83 bb_wallend1in 8 3 2 6 1\r\no93 bb_wallend1in 9 3 2 6 1\r\no103 bb_wallend1in 10 3 2 6 1\r\no113 bb_wallend1in 11 3 2 6 1\r\nn123 bb_crossrd 12 3 2 2 1\r\nn163 bb_crossrd 16 3 2 0 1\r\no173 bb_wallend1in 17 3 2 6 1\r\nn183 bb_crossrd 18 3 2 2 1\r\np34 bb_wallend2in 3 4 1 4 1\r\no74 bb_wallend1in 7 4 1 4 1\r\nb84 bb_bench1 8 4 1 4 2\r\nc94 bb_bench2 9 4 1 4 2\r\nb104 bb_bench1 10 4 1 4 2\r\nc114 bb_bench2 11 4 1 4 2\r\np124 bb_wallend2in 12 4 1 4 1\r\no164 bb_wallend1in 16 4 1 4 1\r\nb174 bb_bench1 17 4 1 4 2\r\nc184 bb_bench2 18 4 1 4 2\r\nm194 bb_wallendout 19 4 1 4 1\r\na75 bb_stool 7 5 1 2 2\r\na125 bb_stool 12 5 1 6 2\r\nb195 bb_bench1 19 5 1 6 2\r\na36 bb_stool 3 6 1 6 2\r\nc196 bb_bench2 19 6 1 6 2\r\nf97 bb_chair 9 7 1 4 2\r\nf107 bb_chair 10 7 1 4 2\r\nb177 bb_bench1 17 7 1 0 2\r\nc187 bb_bench2 18 7 1 0 2\r\nm197 bb_wallendout 19 7 1 0 1\r\na38 bb_stool 3 8 1 6 2\r\nu178 bb_extra 17 8 1 4 1\r\nu188 bb_extra 18 8 1 2 1\r\nn198 bb_crossrd 19 8 1 6 1\r\na39 bb_stool 3 9 1 6 2\r\nf99 bb_chair 9 9 1 0 2\r\nf109 bb_chair 10 9 1 0 2\r\nb179 bb_bench1 17 9 1 4 2\r\nc189 bb_bench2 18 9 1 4 2\r\nm199 bb_wallendout 19 9 1 4 1\r\nb1910 bb_bench1 19 10 1 6 2\r\na711 bb_stool 7 11 1 2 2\r\na1211 bb_stool 12 11 1 6 2\r\nc1911 bb_bench2 19 11 1 6 2\r\no712 bb_wallend1in 7 12 1 0 1\r\nb812 bb_bench1 8 12 1 0 2\r\nc912 bb_bench2 9 12 1 0 2\r\nb1012 bb_bench1 10 12 1 0 2\r\nc1112 bb_bench2 11 12 1 0 2\r\np1212 bb_wallend2in 12 12 1 0 1\r\nb1712 bb_bench1 17 12 1 0 2\r\nc1812 bb_bench2 18 12 1 0 2\r\nm1912 bb_wallendout 19 12 1 0 1\r\nk713 bb_corner1out 7 13 1 4 1\r\nl813 bb_wallout 8 13 1 6 1\r\nl913 bb_wallout 9 13 1 6 1\r\nl1013 bb_wallout 10 13 1 6 1\r\nl1113 bb_wallout 11 13 1 6 1\r\nt1213 bb_special 12 13 1 2 1\r\nm1613 bb_wallendout 16 13 1 6 1\r\nl1713 bb_wallout 17 13 1 6 1\r\nl1813 bb_wallout 18 13 1 6 1\r\nk1913 bb_corner1out 19 13 1 2 1\r\ng914 bb_plant0 9 14 0 6 1\r\nd1014 bb_sofa1 10 14 0 4 2\r\ne1114 bb_sofa2 11 14 0 4 2\r\ni1214 bb_plant2 12 14 0 0 1\r\ni1614 bb_plant2 16 14 0 6 1\r\nd1714 bb_sofa1 17 14 0 4 2\r\ne1814 bb_sofa2 18 14 0 4 2\r\ng1914 bb_plant0 19 14 0 0 1\r\nd915 bb_sofa1 9 15 0 2 2\r\nd1915 bb_sofa1 19 15 0 6 2\r\ne916 bb_sofa2 9 16 0 2 2\r\ne1916 bb_sofa2 19 16 0 6 2\r\nd917 bb_sofa1 9 17 0 2 2\r\nd1917 bb_sofa1 19 17 0 6 2\r\ne918 bb_sofa2 9 18 0 2 2\r\ne1918 bb_sofa2 19 18 0 6 2\r\ng919 bb_plant0 9 19 0 4 1\r\nd1019 bb_sofa1 10 19 0 0 2\r\ne1119 bb_sofa2 11 19 0 0 2\r\nj1219 bb_plant3 12 19 0 2 1\r\nj1619 bb_plant3 16 19 0 4 1\r\nd1719 bb_sofa1 17 19 0 0 2\r\ne1819 bb_sofa2 18 19 0 0 2\r\ng1919 bb_plant0 19 19 0 2 1', '14 19 0');
INSERT INTO `publicrooms` VALUES ('19', '3', 'The Bunny Theatre', 'hh_room_theater_easter', 'theatredrome_easter', 'theater', '0', '60', 'XXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXXXXXXXXXXXXXXXXXX\rXXXXXXX111111111XXXXXXX\rXXXXXXX11111111100000XX\rXXXX00X11111111100000XX\rXXXX00x11111111100000XX\r4XXX00X11111111100000XX\r4440000XXXXXXXXX00000XX\r444000000000000000000XX\r4XX000000000000000000XX\r4XX0000000000000000000X\r44400000000000000000000\r44400000000000000000000\r44X0000000000000000O000\r44X11111111111111111000\r44X11111111111111111000\r33X11111111111111111000\r22X11111111111111111000\r22X11111111111111111000\r22X11111111111111111000\r22X11111111111111111000\r22X11111111111111111000\r22211111111111111111000\r22211111111111111111000\rXXXXXXXXXXXXXXXXXXXX00X\rXXXXXXXXXXXXXXXXXXXX00X\r', 'm1110 mic 11 10 1 0 1\r\nd211 thchair2 2 11 4 2 2\r\nd212 thchair2 2 12 4 2 2\r\nd215 thchair2 2 15 4 2 2\r\nc615 thchair1 6 15 0 0 2\r\nc715 thchair1 7 15 0 0 2\r\nc815 thchair1 8 15 0 0 2\r\nc915 thchair1 9 15 0 0 2\r\nc1015 thchair1 10 15 0 0 2\r\nc1215 thchair1 12 15 0 0 2\r\nc1315 thchair1 13 15 0 0 2\r\nc1415 thchair1 14 15 0 0 2\r\nc1515 thchair1 15 15 0 0 2\r\nc1615 thchair1 16 15 0 0 2\r\nd216 thchair2 2 16 4 2 2\r\nc620 thchair1 6 20 1 0 2\r\nc720 thchair1 7 20 1 0 2\r\nc820 thchair1 8 20 1 0 2\r\nc920 thchair1 9 20 1 0 2\r\nc1020 thchair1 10 20 1 0 2\r\nc1220 thchair1 12 20 1 0 2\r\nc1320 thchair1 13 20 1 0 2\r\nc1420 thchair1 14 20 1 0 2\r\nc1520 thchair1 15 20 1 0 2\r\nc1620 thchair1 16 20 1 0 2\r\nc623 thchair1 6 23 1 0 2\r\nc723 thchair1 7 23 1 0 2\r\nc823 thchair1 8 23 1 0 2\r\nc923 thchair1 9 23 1 0 2\r\nc1023 thchair1 10 23 1 0 2\r\nc1223 thchair1 12 23 1 0 2\r\nc1323 thchair1 13 23 1 0 2\r\nc1423 thchair1 14 23 1 0 2\r\nc1523 thchair1 15 23 1 0 2\r\nc1623 thchair1 16 23 1 0 2\r\nc626 thchair1 6 26 1 0 2\r\nc726 thchair1 7 26 1 0 2\r\nc826 thchair1 8 26 1 0 2\r\nc926 thchair1 9 26 1 0 2\r\nc1026 thchair1 10 26 1 0 2\r\nc1226 thchair1 12 26 1 0 2\r\nc1326 thchair1 13 26 1 0 2\r\nc1426 thchair1 14 26 1 0 2\r\nc1526 thchair1 15 26 1 0 2\r\nc1626 thchair1 16 26 1 0 2', '20 27 0');
INSERT INTO `publicrooms` VALUES ('20', '3', 'The Library', 'hh_room_library', 'library', 'library', '0', '25', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx11111xx1xx1x111111x\rxxxxxxxxxxxx111111xx1xx11111111x\rxx111xxxxxxx111111xx1xx11111111x\rxx111xxxxxxx1111111111111111111x\rxx111xxxxxxx1111111111111111111x\rxx111xxxxxxx1111111111111111111x\rxx111xxxxxxx1111111111111xxxxxxx\rxx111xxxxxx11111111111111111111x\rxx111xxxxxx11111111111111111111x\rxx111xxxxxx11111111111111111111x\rxx111xxxxxx11111111111111xxxxxxx\rxx111xxxxxxxx111111111111111111x\rxx111xx11111x111111111111111111x\rxx111xx11111x111111111111111111x\rxx111xxxxx11x11111111x111xxxxxxx\rxx111xxxxxxxx11111111xx11111111x\rxx111xxx1111111111111xxx1111111x\rxx111xxx1111111111111xxxx111111x\rxxx111xx1111111111x11xxxx000000x\rxxxxx1111xx1111111x11xxxx000000x\rxxxxxxxxxxxx111111x11xxxx000000x\rxxxxxxxxxxxx11xx11x11xxxx000000x\rxxxxxxxxxxxx11xx11x11xxxx000000x\rxxxxxxxxxxxx11xx11x11xxxx000000x\rxxxxxxxxxxxx11xx11x11xxxx000000x\rxxxxxxxxxxxx11xx11x11xxxx000000x\rxxxxxxxxxxxx11xx11x111xxx000000x\rxxxxxxxxxxxxxxxxxxxx11xxx000000x\rxxxxxxxxxxxxxxxxxxxx11xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxx22222xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'a142 libchair 14 2 1 4 2\r\na162 libchair 16 2 1 4 2\r\na262 libchair 26 2 1 4 2\r\na243 libchair 24 3 1 2 2\r\na124 libchair 12 4 1 2 2\r\na126 libchair 12 6 1 2 2\r\nb1314 libstool 13 14 1 6 2\r\nb1315 libstool 13 15 1 6 2\r\nb1316 libstool 13 16 1 6 2\r\na2827 libchair 28 27 0 4 2\r\na2729 libchair 27 29 0 2 2\r\nb2433 libstool 24 33 2 6 2\r\nb2434 libstool 24 34 2 6 2\r\nb2435 libstool 24 35 2 6 2\r\nb2136 libstool 21 36 2 0 2\r\nb2236 libstool 22 36 2 0 2\r\nb2336 libstool 23 36 2 0 2', '20 3 1');
INSERT INTO `publicrooms` VALUES ('21', '3', 'The Floating Garden', 'hh_room_floatinggarden', 'floatinggarden', 'floatinggarden', '0', '65', 'xxxxxxxxxxxxxxxx333333xxxxxxxxx\rxxxxxxxxxxxxxxxx3xxxx3xxxxxxxxx\rxxxxxxxxxxxxxxxx3xxxx3xxxxxxxxx\rxxxxxxxxxxxxxxxx3xxxx3xxxxxxxxx\rxxxxxxxxxxxxxxx223xxx33xxxxxxxx\rxxxxxxxxxxxxxxx11xxx33333xxxxxx\rxxxxxxxxxxxxxxxx11xx3333333xxxx\rxxxxxxxxxxxxxxxx11xx33333333xxx\rxxxxxxxxxxxxxxxxx11xxxxxxxx3xxx\rxxxxxxxxxxxxxxxxxx11xxxx3333xxx\rxxxxxxxxxxxxxxxxxxx1xxxx33333xx\rxxxxxxxxxxxxxxxxxxx1xxx3333333x\r555xxxxxxxxxxx1111111x333333333\r555xxxxxxxxxxx21111111xxxxxx333\r555xxxxxxxxxxx22111111111xxxxxx\r555xxxxxxxxxxx222xxxxxxx111xxxx\r555xxxxxxxxxxx22xxxxxxxxxx1xxxx\r555xxxxxxxxxxx23333333333x111xx\r555xxxxxxxx33333333333333x111xx\r555xxxxxxxx333333x3333333x111xx\r555xxxxxxxx33333333333333x111xx\r555xxxxxxxx33x33333333333x111xx\r555xxxxxxxx33x33x33333333x111xx\r555xxxxxxxx33x33x33333333x111xx\r5554333333333x333x3333333x111xx\rx554333333xxxx33xxxxxxxxxx111xx\rxxxxxxxxx3xxxx333221111111111xx\rxxxxxxxxx3xxxx333221111111111xx\rxxxxxxxxx33333333xx1111x11x11xx\rxxxxxxxxx33333333111xxx11xxxxxx\rxxxxxxxxxxxxxx33311xxxx11xxxxxx\rxxxxxxxxxxxxxx33311xxxx11xxxxxx\rxxxxxxxxxxxxxx333x1xxxx11xxxxxx\rxxxxxxxxxxxxxx333x1xx111111xxxx\rxxxxxxxxxxxxxx33311xx111111xxxx\rxxxxxxxxxx333333311xx111111xxxx\rxxxxxxxxxxx33333311xx111111xxxx\rxxxxxxxxxxxxxxxx111xxxxxxxxxxxx\rxxxxxxxxxxxxxxx111xxxxxxxxxxxxx\r', 'a249 float_dummychair 24 9 3 4 2\r\na259 float_dummychair 25 9 3 4 2\r\nb2813 float_dummychair2 28 13 3 4 2\r\nb2913 float_dummychair2 29 13 3 4 2\r\ne1717 floatbench2 17 17 3 2 2\r\ne1917 floatbench2 19 17 3 6 2\r\ne2117 floatbench2 21 17 3 2 2\r\ne2317 floatbench2 23 17 3 6 2\r\ne2717 floatbench2 27 17 1 4 2\r\nd2817 floatbench1 28 17 1 4 2\r\nd1718 floatbench1 17 18 3 2 2\r\nd1918 floatbench1 19 18 3 6 2\r\nd2118 floatbench1 21 18 3 2 2\r\nd2318 floatbench1 23 18 3 6 2\r\ne2719 floatbench2 27 19 1 0 2\r\nd2819 floatbench1 28 19 1 0 2\r\ne1723 floatbench2 17 23 3 2 2\r\ne1923 floatbench2 19 23 3 6 2\r\ne2123 floatbench2 21 23 3 2 2\r\ne2323 floatbench2 23 23 3 6 2\r\ne2723 floatbench2 27 23 1 4 2\r\nd2823 floatbench1 28 23 1 4 2\r\nd1724 floatbench1 17 24 100000 2 2\r\nd1924 floatbench1 19 24 3 6 2\r\nd2124 floatbench1 21 24 3 2 2\r\nd2324 floatbench1 23 24 3 6 2\r\ne2725 floatbench2 27 25 1 0 2\r\nd2825 floatbench1 28 25 1 0 2\r\na1729 float_dummychair 17 29 1 2 2\r\na1730 float_dummychair 17 30 1 2 2\r\na1731 float_dummychair 17 31 1 2 2\r\na2133 float_dummychair 21 33 1 2 2\r\na2633 float_dummychair 26 33 1 6 2\r\na2134 float_dummychair 21 34 1 2 2\r\na2634 float_dummychair 26 34 1 6 2\r\na1735 float_dummychair 17 35 1 2 2\r\na2135 float_dummychair 21 35 1 2 2\r\na2635 float_dummychair 26 35 1 6 2\r\na1736 float_dummychair 17 36 1 2 2\r\na2136 float_dummychair 21 36 1 2 2\r\na2636 float_dummychair 26 36 1 6 2\r\na1537 float_dummychair 15 37 100000 4 2\r\na1637 float_dummychair 16 37 1 4 2', '2 21 5');
INSERT INTO `rank_fuserights` VALUES ('1', '1', 'default');
INSERT INTO `rank_fuserights` VALUES ('2', '1', 'fuse_login');
INSERT INTO `rank_fuserights` VALUES ('3', '1', 'fuse_buy_credits');
INSERT INTO `rank_fuserights` VALUES ('4', '1', 'fuse_trade');
INSERT INTO `rank_fuserights` VALUES ('5', '1', 'fuse_room_queue_default');
INSERT INTO `rank_fuserights` VALUES ('6', '2', 'fuse_extended_buddylist');
INSERT INTO `rank_fuserights` VALUES ('7', '2', 'fuse_habbo_chooser');
INSERT INTO `rank_fuserights` VALUES ('8', '2', 'fuse_furni_chooser');
INSERT INTO `rank_fuserights` VALUES ('9', '2', 'fuse_room_queue_club');
INSERT INTO `rank_fuserights` VALUES ('10', '2', 'fuse_priority_access');
INSERT INTO `rank_fuserights` VALUES ('11', '2', 'fuse_use_special_room_layouts');
INSERT INTO `rank_fuserights` VALUES ('12', '2', 'fuse_use_club_dance');
INSERT INTO `rank_fuserights` VALUES ('13', '3', 'fuse_enter_full_rooms');
INSERT INTO `rank_fuserights` VALUES ('14', '4', 'fuse_enter_locked_rooms');
INSERT INTO `rank_fuserights` VALUES ('16', '4', 'fuse_kick');
INSERT INTO `rank_fuserights` VALUES ('17', '4', 'fuse_mute');
INSERT INTO `rank_fuserights` VALUES ('18', '5', 'fuse_ban');
INSERT INTO `rank_fuserights` VALUES ('19', '5', 'fuse_room_mute');
INSERT INTO `rank_fuserights` VALUES ('20', '5', 'fuse_room_kick');
INSERT INTO `rank_fuserights` VALUES ('21', '5', 'fuse_receive_calls_for_help');
INSERT INTO `rank_fuserights` VALUES ('22', '5', 'fuse_remove_stickies');
INSERT INTO `rank_fuserights` VALUES ('23', '6', 'fuse_mod');
INSERT INTO `rank_fuserights` VALUES ('24', '6', 'fuse_superban');
INSERT INTO `rank_fuserights` VALUES ('25', '6', 'fuse_pick_up_any_furni');
INSERT INTO `rank_fuserights` VALUES ('26', '6', 'fuse_ignore_room_owner');
INSERT INTO `rank_fuserights` VALUES ('27', '6', 'fuse_any_room_controller');
INSERT INTO `rank_fuserights` VALUES ('28', '3', 'fuse_room_alert');
INSERT INTO `rank_fuserights` VALUES ('29', '6', 'fuse_moderator_access');
INSERT INTO `rank_fuserights` VALUES ('30', '7', 'fuse_administrator_access');
INSERT INTO `rank_fuserights` VALUES ('31', '7', 'fuse_see_flat_ids');
INSERT INTO `rank_fuserights` VALUES ('15', '5', 'fuse_enter_all_rooms');
INSERT INTO `rank_fuserights` VALUES ('33', '6', 'fuse_performance_panel');
INSERT INTO `rank_fuserights` VALUES ('34', '4', 'fuse_alert');
INSERT INTO `ranks` VALUES ('1', 'normal', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `ranks` VALUES ('2', 'clubsubscriber', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `ranks` VALUES ('3', 'expert', '0', '0', '1', '0', '0', '0', '0');
INSERT INTO `ranks` VALUES ('4', 'silver', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `ranks` VALUES ('5', 'gold', '0', '1', '1', '1', '1', '1', '1');
INSERT INTO `ranks` VALUES ('6', 'moderator', '1', '1', '1', '1', '1', '1', '1');
INSERT INTO `ranks` VALUES ('7', 'administrator', '1', '1', '1', '1', '1', '1', '1');
INSERT INTO `roombots` VALUES ('1', '20', 'Barrack Obahma', 'sd=001&sh=001/194,227,232&lg=001/255,255,255&ch=001/255,255,255&lh=001/240,213,179&rh=001/240,213,179&hd=001/240,213,179&ey=001/254,202,150&fc=001/240,213,179&hr=888/255,255,255&rs=001/255,255,255&ls=001/255,255,255&bd=001/240,213,179', 'Bush put me here! Heh!', '', '');
INSERT INTO `sslevels` VALUES ('13', '3', 'Welcome Lounge', 'hh_room_nlobby', 'welcome_lounge', 'newbie_lobby 15659', '0', '35', 'xxxxxxxxxxxxxxxx000000\rxxxxx0xxxxxxxxxx000000\rxxxxx00000000xxx000000\rxxxxx000000000xx000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\r0000000000000000000000\rxxxxx000000000000000xx\rxxxxx000000000000000xx\rx0000000000000000000xx\rx0000000000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxxx0000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxxxx000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxxx00000000000xxxxx\rxxxxx000000000000xxxxx\rxxxxx000000000000xxxxx\r', 'a160 crl_lamp 16 0 0 0 1\r\ny170 crl_sofa2c 17 0 0 4 2\r\nw180 crl_sofa2b 18 0 0 4 2\r\nv190 crl_sofa2a 19 0 0 4 2\r\na200 crl_lamp 20 0 0 0 1\r\nb161 crl_chair 16 1 0 2 2\r\na72 crl_lamp 7 2 0 0 1\r\na112 crl_lamp 11 2 0 0 1\r\nb162 crl_chair 16 2 0 2 2\r\nc53 crl_pillar 5 3 0 0 1\r\nb73 crl_chair 7 3 0 2 2\r\nu93 crl_table1b 9 3 0 0 1\r\ns113 crl_sofa1c 11 3 0 6 2\r\nb163 crl_chair 16 3 0 2 2\r\nA193 crl_table2b 19 3 0 0 1\r\nz203 crl_table2a 20 3 0 0 1\r\na04 crl_lamp 0 4 0 0 1\r\ny14 crl_sofa2c 1 4 0 4 2\r\nw24 crl_sofa2b 2 4 0 4 2\r\nv34 crl_sofa2a 3 4 0 4 2\r\na44 crl_lamp 4 4 0 0 1\r\nt94 crl_table1a 9 4 0 0 1\r\nr114 crl_sofa1b 11 4 0 6 2\r\nh154 crl_wall2a 15 4 0 0 1\r\na164 crl_lamp 16 4 0 0 1\r\nb05 crl_chair 0 5 0 2 2\r\nb75 crl_chair 7 5 0 2 2\r\nq115 crl_sofa1a 11 5 0 6 2\r\nA26 crl_table2b 2 6 0 0 1\r\nz36 crl_table2a 3 6 0 0 1\r\na116 crl_lamp 11 6 0 0 1\r\nb07 crl_chair 0 7 0 2 2\r\na08 crl_lamp 0 8 0 0 1\r\nD18 crl_sofa3c 1 8 0 0 2\r\nC28 crl_sofa3b 2 8 0 0 2\r\nB38 crl_sofa3a 3 8 0 0 2\r\na48 crl_lamp 4 8 0 0 1\r\no198 crl_barchair2 19 8 0 0 2\r\np208 crl_tablebar 20 8 0 0 1\r\no218 crl_barchair2 21 8 0 0 2\r\nE59 crl_pillar2 5 9 0 0 1\r\nc99 crl_pillar 9 9 0 0 1\r\nP815 crl_desk1a 8 15 0 0 1\r\nO915 crl_deski 9 15 0 0 1\r\nN1015 crl_deskh 10 15 0 0 1\r\nM1016 crl_deskg 10 16 0 0 1\r\nL1017 crl_deskf 10 17 0 0 1\r\nK1018 crl_deske 10 18 0 0 1\r\nK1019 crl_deske 10 19 0 0 1\r\nK1020 crl_deske 10 20 0 0 1\r\nK1021 crl_deske 10 21 0 0 1\r\nK1022 crl_deske 10 22 0 0 1\r\nK1023 crl_deske 10 23 0 0 1\r\nG724 crl_wallb 7 24 0 0 1\r\nK1024 crl_deske 10 24 0 0 1\r\nF725 crl_walla 7 25 0 0 1\r\nH825 crl_desk1b 8 25 0 0 1\r\nI925 crl_desk1c 9 25 0 0 1\r\nJ1025 crl_desk1d 10 25 0 0 1\r\nd1227 crl_lamp2 12 27 0 0 1\r\nf1327 crl_cabinet2 13 27 0 0 1\r\ne1427 crl_cabinet1 14 27 0 0 1\r\nd1527 crl_lamp2 15 27 0 0 1', '2 11 0');
INSERT INTO `sslevels` VALUES ('14', '3', 'The Lido', 'hh_room_pool,hh_people_pool', 'habbo_lido', 'pool_a 15697', '0', '70', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx7xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx777xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx7777777xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx777777777xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx777777xxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7x777777777xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx77777777xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7x777777777x7xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx7xxx7777777777xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx777777777777xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777777x2111xxxxxxxxxxxx\rxxxxxxxxxxxxxxx7777777777x221111xxxxxxxxxxx\rxxxxxxxxx7777777777777777x2211111xxxxxxxxxx\rxxxxxxxxx7777777777777777x22211111xxxxxxxxx\rxxxxxxxxx7777777777777777x222211111xxxxxxxx\rxxxxxx77777777777777777777x222211111xxxxxxx\rxxxxxx7777777xx777777777777x222211111xxxxxx\rxxxxxx7777777xx77777777777772222111111xxxxx\rxxxxxx777777777777777777777x22221111111xxxx\rxx7777777777777777777777x322222211111111xxx\r77777777777777777777777x33222222111111111xx\r7777777777777777777777x333222222211111111xx\rxx7777777777777777777x333322222222111111xxx\rxx7777777777777777777333332222222221111xxxx\rxx777xxx777777777777733333222222222211xxxxx\rxx777x7x77777777777773333322222222222xxxxxx\rxx777x7x7777777777777x33332222222222xxxxxxx\rxxx77x7x7777777777777xx333222222222xxxxxxxx\rxxxx77777777777777777xxx3222222222xxxxxxxxx\rxxxxx777777777777777777xx22222222xxxxxxxxxx\rxxxxxx777777777777777777x2222222xxxxxxxxxxx\rxxxxxxx777777777777777777222222xxxxxxxxxxxx\rxxxxxxxx7777777777777777722222xxxxxxxxxxxxx\rxxxxxxxxx77777777777777772222xxxxxxxxxxxxxx\rxxxxxxxxxx777777777777777222xxxxxxxxxxxxxxx\rxxxxxxxxxxx77777777777777x2xxxxxxxxxxxxxxxx\rxxxxxxxxxxxx77777777777777xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx777777777777xxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx7777777777xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx77777777xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx777777xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx7777xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx77xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'u184 umbrella2 18 4 7 0 1\r\nb185 pool_2sofa2 18 5 7 2 2\r\nB186 pool_2sofa1 18 6 7 2 2\r\nu720 umbrella2 7 20 7 0 1\r\nc820 pool_chair2 8 20 7 4 2\r\nu2420 umbrella2 24 20 7 0 1\r\nc2520 pool_chair2 25 20 7 4 2\r\nc721 pool_chair2 7 21 7 2 2\r\nt821 pool_table2 8 21 7 0 2\r\nc921 pool_chair2 9 21 7 6 2\r\nc2421 pool_chair2 24 21 7 2 2\r\nt2521 pool_table2 25 21 7 0 1\r\nc822 pool_chair2 8 22 7 0 2\r\nR228 flower2b 2 28 7 0 1\r\nr229 flower2a 2 29 7 0 1\r\nL632 box 6 32 7 0 1\r\nf1333 flower1 13 33 7 0 1\r\ny1034 pool_chairy 10 34 7 4 2\r\n8835 umbrellay 8 35 7 0 1\r\ny935 pool_chairy 9 35 7 2 2 2\r\nQ1035 pool_tabley 10 35 7 0 1\r\ny1135 pool_chairy 11 35 7 6 2\r\n91535 umbrellap 15 35 7 0 1\r\n72135 umbrellao 21 35 7 0 1\r\ny1036 pool_chairy 10 36 7 0 2\r\nP1536 pool_chairp 15 36 7 4 2\r\no2136 pool_chairo 21 36 7 4 2\r\no2236 pool_chairo 22 36 7 4 2\r\nP1437 pool_chairp 14 37 7 2 2\r\nZ1537 pool_tablep 15 37 7 0 1\r\nP1637 pool_chairp 16 37 7 6 2\r\nK2137 pool_tabo 21 37 7 6 1\r\nk2237 pool_tabo2 22 37 7 6 1\r\nP1438 pool_chairp 14 38 7 2 2\r\nz1538 pool_tablep2 15 38 7 0 1\r\nP1638 pool_chairp 16 38 7 6 2\r\no2138 pool_chairo 21 38 7 0 2\r\no2238 pool_chairo 22 38 7 0 2\r\nP1539 pool_chairp 15 39 7 0 2\r\ng2041 pool_chairg 20 41 7 4 2\r\ng2141 pool_chairg 21 41 7 4 2\r\nW2042 pool_tablg 20 42 7 6 1\r\nw2142 pool_tablg2 21 42 7 6 1\r\nG1943 umbrellag 19 43 7 0 1\r\ng2043 pool_chairg 20 43 7 0 2\r\ng2143 pool_chairg 21 43 7 0 2', '2 25 7');
INSERT INTO `sslevels` VALUES ('15', '3', 'Holo - Oldskool', 'hh_room_old_skool', 'old_skool', 'old_skool1 13243', '0', '20', 'x6666666665432100\rx6666666665432100\rx6600000000000x00\rx6600000000000000\rx6600000000000000\rx6600000000000000\rx660000000000x000\r666000000000x1111\rx66000000000xx111\rx66000000000x1111\rx66000000000x1111\rx55000000000x1111\rx44000000000x1111\rx33000000000x1111\rx22000000000xx111\rx11x00000000x1111\rx00000000000x1111\rx00000000000xx111', 'j42 mobiles_chair2 4 2 0 4 2\r\nj92 mobiles_chair2 9 2 0 4 2\r\ng43 mobiles_table6 4 3 0 0 1\r\nh53 mobiles_table7 5 3 0 0 1\r\nj73 mobiles_chair2 7 3 0 2 2\r\ng83 mobiles_table6 8 3 0 0 1\r\nh93 mobiles_table7 9 3 0 0 1\r\nj34 mobiles_chair2 3 4 0 2 2\r\nf44 mobiles_table5 4 4 0 0 1\r\ni54 mobiles_table8 5 4 0 0 1\r\nf84 mobiles_table5 8 4 0 0 1\r\ni94 mobiles_table8 9 4 0 0 1\r\nj85 mobiles_chair2 8 5 0 0 2\r\nj56 mobiles_chair2 5 6 0 4 2\r\nj37 mobiles_chair2 3 7 0 2 2\r\ng47 mobiles_table6 4 7 0 0 1\r\nh57 mobiles_table7 5 7 0 0 1\r\nf48 mobiles_table5 4 8 0 0 1\r\ni58 mobiles_table8 5 8 0 0 1\r\nj49 mobiles_chair2 4 9 0 0 2', '1 7 6');
INSERT INTO `sslevels` VALUES ('16', '3', 'SnowStorm lobby', 'hh_gamesys,hh_game_snowwar,hh_game_snowwar_room,hh_game_snowwar_ui', 'sw_lobby_amateur_6', 'snowwar_lobby_1 15812', '0', '10', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx11111xx1xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx11111xx1111xxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx1111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx111111111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx1111x1111111111xxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'a3118 sw_barrellchair 31 18 1 6 2\r\na3119 sw_barrellchair 31 19 1 6 2\r\na3020 sw_barrellchair 30 20 1 0 2\r\na3720 sw_barrellchair 37 20 1 0 2\r\na3920 sw_barrellchair 39 20 1 0 2\r\na4120 sw_barrellchair 41 20 1 0 2\r\nd3024 sw_chair1 30 24 1 4 2\r\nc3124 sw_chair2 31 24 1 4 2\r\nc3224 sw_chair2 32 24 1 4 2\r\nc3324 sw_chair2 33 24 1 4 2\r\nb3424 sw_chair3 34 24 1 4 2\r\ni3025 sw_table1 30 25 1 0 1\r\nh3125 sw_table2 31 25 1 0 1\r\ng3225 sw_table3 32 25 1 0 1\r\nf3325 sw_table4 33 25 1 0 1\r\ne3425 sw_table5 34 25 1 0 1\r\nd3026 sw_chair1 30 26 1 0 2\r\nc3126 sw_chair2 31 26 1 0 2\r\nc3226 sw_chair2 32 26 1 0 2\r\nc3326 sw_chair2 33 26 1 0 2\r\nb3426 sw_chair3 34 26 1 0 2\r\nd3029 sw_chair1 30 29 1 4 2\r\nc3129 sw_chair2 31 29 1 4 2\r\nc3229 sw_chair2 32 29 1 4 2\r\nc3329 sw_chair2 33 29 1 4 2\r\nb3429 sw_chair3 34 29 1 4 2\r\ni3030 sw_table1 30 30 1 0 1\r\nh3130 sw_table2 31 30 1 0 1\r\ng3230 sw_table3 32 30 1 0 1\r\nf3330 sw_table4 33 30 1 0 1\r\ne3430 sw_table5 34 30 1 0 1\r\nd3031 sw_chair1 30 31 1 0 2\r\nc3131 sw_chair2 31 31 1 0 2\r\nc3231 sw_chair2 32 31 1 0 2\r\nc3331 sw_chair2 33 31 1 0 2\r\nb3431 sw_chair3 34 31 1 0 2\r\nx2732 invisichair 27 32 1 2 2\r\nx2733 invisichair 27 33 1 2 2\r\nx2734 invisichair 27 34 1 2 2\r\nx2836 invisichair 28 36 1 0 2\r\nx2936 invisichair 29 36 1 0 2\r\nx3036 invisichair 30 36 1 0 2\r\nx3136 invisichair 31 36 1 0 2', '41 36 1');
INSERT INTO `sslevels` VALUES ('91', '0', 'Artic Island', null, null, 'snowwar_arena_0 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx0000000000000000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000000000000000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxx0000000000000000000000000xxxxxxxxxxxxxxxx\rxxxxxxxx000000000000000000000000000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000000000000xxxxxxxxxxxxxx\rxxxxxx0000000000000000000000000000000xxxxxxxxxxxxx\rxxxxx000000000000000000000000000000000xxxxxxxxxxxx\rxxxx00000000000000000000000000000000000xxxxxxxxxxx\rxxx0000000000000000000000000000000000000xxxxxxxxxx\rxx000000000000000000000000000000000000000xxxxxxxxx\rx00000000000000000000000000000000000000000xxxxxxxx\r00000000000000000000xxxx0xxxxxx000000000000xxxxxxx\r00000000000000000000xxxx0xxxxxxx000000000000xxxxxx\r00000000000000000000xxxx0xxxxxxx0000000000000xxxxx\r00000000000000000000xxx000000xxx00000000000000xxxx\rx0000000000000000000xxx000000xxx000000000000000xxx\rxx000000000000000000xxx0000000000000000000000000xx\rxxx00000000000000000xxx000000xxx00000000000000000x\rxxxx0000000000000000000000000xxx000000000000000000\rxxxxx000000000000000xxx000000xxx000000000000000000\rxxxxxx00000000000000xxxxxxx0xxxx000000000000000000\rxxxxxxx0000000000000xxxxxxx0xxxx000000000000000000\rxxxxxxxx0000000000000xxxxxx0xxx0000000000000000000\rxxxxxxxxx00000000000000000000000000000000000000000\rxxxxxxxxxx000000000000000000000000000000000000000x\rxxxxxxxxxxx0000000000000000000000000000000000000xx\rxxxxxxxxxxxx00000000000000000000000000000000000xxx\rxxxxxxxxxxxxx000000000000000000000000000000000xxxx\rxxxxxxxxxxxxxx0000000000000000000000000000000xxxxx\rxxxxxxxxxxxxxxx00000000000000000000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000000000000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000000000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx00000000000000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx000000000000000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx0000000000000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxx\r', 'n280 sw_fence 28 0 100000 2\r\nn281 sw_fence 28 1 100000 2\r\nn282 sw_fence 28 2 100000 2\r\ng223 sw_tree1 22 3 0 1\r\nn283 sw_fence 28 3 100000 2\r\nn284 sw_fence 28 4 100000 2\r\nj275 sw_tree4 27 5 0 1\r\nn285 sw_fence 28 5 0 2\r\nn286 sw_fence 28 6 0 2\r\nh227 sw_tree2 22 7 0 1\r\nn287 sw_fence 28 7 0 2\r\nn288 sw_fence 28 8 0 2\r\nn289 sw_fence 28 9 0 2\r\ng1610 sw_tree1 16 10 0 1\r\nn2810 sw_fence 28 10 0 2\r\nj3413 sw_tree4 34 13 0 1\r\nn1314 sw_fence 13 14 0 4\r\nn1414 sw_fence 14 14 0 4\r\nn1514 sw_fence 15 14 0 4\r\nn1614 sw_fence 16 14 0 4\r\nn1714 sw_fence 17 14 0 4\r\nn1814 sw_fence 18 14 0 4\r\nn1914 sw_fence 19 14 0 4\r\nn2014 sw_fence 20 14 0 4\r\nn2114 sw_fence 21 14 0 4\r\nn2214 sw_fence 22 14 0 4\r\nh1315 sw_tree2 13 15 0 1\r\nb218 block_basic2 2 18 0 1\r\nb318 block_basic2 3 18 0 1\r\nb418 block_basic2 4 18 0 1\r\nb518 block_basic2 5 18 0 1\r\nb618 block_basic2 6 18 0 1\r\nb718 block_basic2 7 18 0 1\r\nb818 block_basic2 8 18 0 1\r\nb918 block_basic2 9 18 0 1\r\nb1018 block_basic2 10 18 0 1\r\nb1118 block_basic2 11 18 0 1\r\na1218 block_basic 12 18 0 1\r\nb219 block_basic2 2 19 0 1\r\nv020 sw_backround2 0 20 0 0\r\nb220 block_basic2 2 20 0 1\r\ng620 sw_tree1 6 20 0 1\r\nb3820 block_basic2 38 20 0 1\r\nb3920 block_basic2 39 20 0 1\r\nb4020 block_basic2 40 20 0 1\r\nb4120 block_basic2 41 20 0 1\r\nb4220 block_basic2 42 20 0 1\r\nb4320 block_basic2 43 20 0 1\r\nb221 block_basic2 2 21 0 1\r\nb3821 block_basic2 38 21 0 1\r\nb222 block_basic2 2 22 0 1\r\nb3822 block_basic2 38 22 0 1\r\nb3823 block_basic2 38 23 0 1\r\nh524 sw_tree2 5 24 0 1\r\nb3824 block_basic2 38 24 0 1\r\ng4525 sw_tree1 45 25 0 1\r\ni1026 sw_tree3 10 26 0 1\r\nh4928 sw_tree2 49 28 0 1\r\ni4732 sw_tree3 47 32 0 1\r\nj1534 sw_tree4 15 34 0 1\r\na3737 block_basic 37 37 0 1\r\nb3837 block_basic2 38 37 0 1\r\nb3937 block_basic2 39 37 0 1\r\nb4037 block_basic2 40 37 0 1\r\nb4137 block_basic2 41 37 0 1\r\na4237 block_basic 42 37 0 1\r\nb2338 block_basic2 23 38 0 1\r\nn2438 sw_fence 24 38 0 2\r\ng2538 sw_tree1 25 38 0 1\r\na2339 block_basic 23 39 0 1\r\nn2439 sw_fence 24 39 0 2\r\nb2340 block_basic2 23 40 0 1\r\nn2440 sw_fence 24 40 0 2\r\nh1941 sw_tree2 19 41 0 1\r\na2341 block_basic 23 41 0 1\r\nn2441 sw_fence 24 41 0 2\r\nb2342 block_basic2 23 42 0 1\r\nn2442 sw_fence 24 42 0 2\r\na2343 block_basic 23 43 0 1\r\nn2443 sw_fence 24 43 0 2\r\nb2344 block_basic2 23 44 0 1\r\nn2444 sw_fence 24 44 0 2\r\na2345 block_basic 23 45 0 1\r\nn2445 sw_fence 24 45 0 2\r\nb2346 block_basic2 23 46 100000 1\r\nn2446 sw_fence 24 46 0 2\r\ni2847 sw_tree3 28 47 0 1\r\nh3447 sw_tree2 34 47 100000 1', null);
INSERT INTO `sslevels` VALUES ('92', '0', 'Algid River', null, null, 'snowwar_arena_1 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx0000000000000000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000000000000000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxx0000000000000000000000000xxxxxxxxxxxxxxxx\rxxxxxxxx000000000000000000000000000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000000000000xxxxxxxxxxxxxx\rxxxxxx0000000000000000000000000000000xxxxxxxxxxxxx\rxxxxx000000000000000000000000000000000xxxxxxxxxxxx\rxxxx00000000000000000000000000000000000xxxxxxxxxxx\rxxx0000000000000000000000000000000000000xxxxxxxxxx\rxx000000000000000000000000000000000000000xxxxxxxxx\rx00000000000000000000000000000000000000000xxxxxxxx\r0000000000000000000000000000000000000000000xxxxxxx\r0000000000000000000xxxxxxxxxx0xxxxxx00000000xxxxxx\r0000000000000000000xxxxxxxxxx0xxxxxxx00000000xxxxx\r0000000000000000000xxxxxxxxxx0xxxxxxx000000000xxxx\rx000000000000000000xxx000000000000xxx0000000000xxx\rxx00000000000000000xxx000000000000xxx00000000000xx\rxxx0000000000000000xxx000000000000xxx000000000000x\rxxxx000000000000000xxx000000000000xxx0000000000000\rxxxxx00000000000000000000000000000xxx0000000000000\rxxxxxx0000000000000xxx000000000000xxxx000000000000\rxxxxxxx000000000000xxx000000000000xxxxxxxxxx0xxxxx\rxxxxxxxx00000000000xxx000000000000xxxxxxxxxx0xxxxx\rxxxxxxxxx0000000000xxx0000000000000xxxxxxxxx0xxxxx\rxxxxxxxxxx000000000xxxxxxxx0000000000000000000000x\rxxxxxxxxxxx00000000xxxxxxxxx00000000000000000000xx\rxxxxxxxxxxxx00000000xxxxxxxx0000000000000000000xxx\rxxxxxxxxxxxxx000000000000xxx000000000000000000xxxx\rxxxxxxxxxxxxxx00000000000xxx00000000000000000xxxxx\rxxxxxxxxxxxxxxx0000000000xxx0000000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000xxx000000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx00000000xxx00000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxx0000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx000000xxx000000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx00000xxx00000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx0000xxx0000000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000xxx000000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx00xxx00000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx0xxx0000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx000000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxx0000xxxxxxxxxxxxxxxxxx\r', 'j233 sw_tree4 23 3 0 1\r\nn283 sw_fence 28 3 100000 2\r\nn284 sw_fence 28 4 100000 2\r\nn285 sw_fence 28 5 0 2\r\nn286 sw_fence 28 6 0 2\r\nn287 sw_fence 28 7 0 2\r\nh297 sw_tree2 29 7 0 1\r\ni198 sw_tree3 19 8 0 1\r\nn288 sw_fence 28 8 0 2\r\nn289 sw_fence 28 9 0 2\r\nn2810 sw_fence 28 10 0 2\r\ng2910 sw_tree1 29 10 0 1\r\nn2811 sw_fence 28 11 0 2\r\nn2812 sw_fence 28 12 0 2\r\nn2813 sw_fence 28 13 0 2\r\nn2814 sw_fence 28 14 0 2\r\nn2815 sw_fence 28 15 0 2\r\nn2816 sw_fence 28 16 0 2\r\nn2817 sw_fence 28 17 0 2\r\ng4018 sw_tree1 40 18 0 1\r\nj319 sw_tree4 3 19 0 1\r\nn719 sw_fence 7 19 0 4\r\nn819 sw_fence 8 19 0 4\r\nn919 sw_fence 9 19 0 4\r\nn1019 sw_fence 10 19 0 4\r\nn1119 sw_fence 11 19 0 4\r\nn1219 sw_fence 12 19 0 4\r\nn1319 sw_fence 13 19 0 4\r\nn1419 sw_fence 14 19 0 4\r\nn1519 sw_fence 15 19 0 4\r\nn1619 sw_fence 16 19 0 4\r\nn1719 sw_fence 17 19 0 4\r\nw020 sw_backround3 0 20 0 0\r\nh620 sw_tree2 6 20 0 1\r\nh2424 sw_tree2 24 24 0 1\r\ni926 sw_tree3 9 26 0 1\r\ni3926 sw_tree3 39 26 0 1\r\nj4627 sw_tree4 46 27 0 1\r\ng2329 sw_tree1 23 29 0 1\r\na3329 block_basic 33 29 0 1\r\na3330 block_basic 33 30 0 1\r\na3331 block_basic 33 31 0 1\r\na2832 block_basic 28 32 0 1\r\na2932 block_basic 29 32 0 1\r\na3032 block_basic 30 32 0 1\r\nh1837 sw_tree2 18 37 0 1\r\ni4238 sw_tree3 42 38 0 1\r\nj3249 sw_tree4 32 49 100000 1', null);
INSERT INTO `sslevels` VALUES ('93', '0', 'Glacial Fort', null, null, 'snowwar_arena_2 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx0000000000000000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000000000000000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxx0000000000000000000000000xxxxxxxxxxxxxxxx\rxxxxxxxx000000000000000000000000000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000000000000xxxxxxxxxxxxxx\rxxxxxx0000000000000000000000000000000xxxxxxxxxxxxx\rxxxxx000000000000000000000000000000000xxxxxxxxxxxx\rxxxx00000000000000000000000000000000000xxxxxxxxxxx\rxxx0000000000000000000000000000000000000xxxxxxxxxx\rxx000000000000000000000000000000000000000xxxxxxxxx\rx00000000000000000000000000000000000000000xxxxxxxx\r0000000000000000000000000000000000000000000xxxxxxx\r00000000000000000000000000000000000000000000xxxxxx\r000000000000000000000000000000000000000000000xxxxx\r0000000000000000000000000000000000000000000000xxxx\rx0000000000000000000000000000000000000000000000xxx\rxx0000000000000000000000000000000000000000000000xx\rxxx0000000000000000000000000000000000000000000000x\rxxxx0000000000000000000000000000000000000000000000\rxxxxx000000000000000000000000000000000000000000000\rxxxxxx00000000000000000000000000000000000000000000\rxxxxxxx0000000000000000000000000000000000000000000\rxxxxxxxx000000000000000000000000000000000000000000\rxxxxxxxxx00000000000000000000000000000000000000000\rxxxxxxxxxx000000000000000000000000000000000000000x\rxxxxxxxxxxx0000000000000000000000000000000000000xx\rxxxxxxxxxxxx00000000000000000000000000000000000xxx\rxxxxxxxxxxxxx000000000000000000000000000000000xxxx\rxxxxxxxxxxxxxx0000000000000000000000000000000xxxxx\rxxxxxxxxxxxxxxx00000000000000000000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000000000000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000000000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx00000000000000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx000000000000000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx0000000000000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxx\r', 'g212 sw_tree1 21 2 0 1\r\nh232 sw_tree2 23 2 0 1\r\ni264 sw_tree3 26 4 0 1\r\nj157 sw_tree4 15 7 0 1\r\nb207 block_basic2 20 7 0 1\r\nb208 block_basic2 20 8 0 1\r\nb298 block_basic2 29 8 0 1\r\nb209 block_basic2 20 9 0 1\r\nb299 block_basic2 29 9 0 1\r\nb2010 block_basic2 20 10 0 1\r\nb2910 block_basic2 29 10 0 1\r\ni1511 sw_tree3 15 11 0 1\r\nb2011 block_basic2 20 11 0 1\r\nb2911 block_basic2 29 11 0 1\r\nb2012 block_basic2 20 12 0 1\r\nb2112 block_basic2 21 12 0 1\r\nb2212 block_basic2 22 12 0 1\r\na2312 block_basic 23 12 0 0\r\na2612 block_basic 26 12 0 1\r\nb2712 block_basic2 27 12 0 1\r\nb2812 block_basic2 28 12 0 1\r\nb2912 block_basic2 29 12 0 1\r\nh3312 sw_tree2 33 12 0 1\r\na2313 block_basic 23 13 0 1\r\na2613 block_basic 26 13 0 1\r\na1217 block_basic 12 17 0 1\r\nb1218 block_basic2 12 18 0 1\r\nb1219 block_basic2 12 19 0 1\r\nb1220 block_basic2 12 20 0 1\r\nb1221 block_basic2 12 21 0 1\r\ng622 sw_tree1 6 22 0 1\r\nc1222 block_basic3 12 22 0 1\r\nb1322 block_basic2 13 22 0 1\r\na1422 block_basic 14 22 0 1\r\ng3522 sw_tree1 35 22 0 1\r\nb4122 block_basic2 41 22 0 1\r\nb4222 block_basic2 42 22 0 1\r\na4322 block_basic 43 22 0 1\r\nc4123 block_basic3 41 23 0 1\r\nh324 sw_tree2 3 24 0 1\r\nc1224 block_basic3 12 24 0 1\r\nb1324 block_basic2 13 24 0 1\r\na1424 block_basic 14 24 0 1\r\na2424 block_basic 24 24 0 1\r\na2524 block_basic 25 24 0 1\r\na2624 block_basic 26 24 0 1\r\na2724 block_basic 27 24 0 1\r\na2824 block_basic 28 24 0 1\r\nb4124 block_basic2 41 24 0 1\r\nb1225 block_basic2 12 25 0 1\r\na2425 block_basic 24 25 0 1\r\na2825 block_basic 28 25 0 1\r\nc4125 block_basic3 41 25 0 1\r\nb1226 block_basic2 12 26 0 1\r\na2426 block_basic 24 26 0 1\r\nk2626 obst_duck 26 26 0 1\r\na2826 block_basic 28 26 0 1\r\nb4126 block_basic2 41 26 0 1\r\nj527 sw_tree4 5 27 0 1\r\nb1227 block_basic2 12 27 0 1\r\na2427 block_basic 24 27 0 1\r\na2827 block_basic 28 27 0 1\r\nc4127 block_basic3 41 27 0 1\r\nb4227 block_basic2 42 27 0 1\r\na4327 block_basic 43 27 0 1\r\nb1228 block_basic2 12 28 0 1\r\na2428 block_basic 24 28 0 1\r\na2528 block_basic 25 28 0 1\r\na2628 block_basic 26 28 0 1\r\na2728 block_basic 27 28 0 1\r\na2828 block_basic 28 28 0 1\r\nb1229 block_basic2 12 29 0 1\r\na1230 block_basic 12 30 0 1\r\ni1530 sw_tree3 15 30 0 1\r\nc4130 block_basic3 41 30 0 1\r\nb4230 block_basic2 42 30 0 1\r\na4330 block_basic 43 30 0 1\r\nb4131 block_basic2 41 31 0 1\r\nc4132 block_basic3 41 32 0 1\r\nb4133 block_basic2 41 33 0 1\r\nc4134 block_basic3 41 34 0 1\r\nb4135 block_basic2 41 35 0 1\r\nb4235 block_basic2 42 35 0 1\r\na4335 block_basic 43 35 0 1\r\ng1638 sw_tree1 16 38 0 1\r\nh2040 sw_tree2 20 40 0 1\r\nb2441 block_basic2 24 41 0 0\r\nb2541 block_basic2 25 41 0 0\r\nc2641 block_basic3 26 41 0 0\r\nb2741 block_basic2 27 41 0 1\r\nd2841 block_arch1 28 41 0 1\r\nf3041 block_arch3 30 41 0 1\r\nb3141 block_basic2 31 41 0 1\r\nc3241 block_basic3 32 41 0 1\r\nb3341 block_basic2 33 41 0 1\r\nb3441 block_basic2 34 41 0 1\r\nb2442 block_basic2 24 42 0 0\r\nb3442 block_basic2 34 42 0 1\r\nj3842 sw_tree4 38 42 0 1\r\nb2443 block_basic2 24 43 0 0\r\nb3443 block_basic2 34 43 0 1\r\na2444 block_basic 24 44 0 0\r\nb3444 block_basic2 34 44 0 1\r\na3445 block_basic 34 45 0 1', null);
INSERT INTO `sslevels` VALUES ('94', '0', 'Frosty Forest', null, null, 'snowwar_arena_3 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx0000000000000000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000000000000000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxx0000000000000000000000000xxxxxxxxxxxxxxxx\rxxxxxxxx000000000000000000000000000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000000000000xxxxxxxxxxxxxx\rxxxxxx0000000000000000000000000000000xxxxxxxxxxxxx\rxxxxx000000000000000000000000000000000xxxxxxxxxxxx\rxxxx00000000000000000000000000000000000xxxxxxxxxxx\rxxx0000000000000000000000000000000000000xxxxxxxxxx\rxx000000000000000000000000000000000000000xxxxxxxxx\rx00000000000000000000000000000000000000000xxxxxxxx\r0000000000000000000000000000000000000000000xxxxxxx\r00000000000000000000000000000000000000000000xxxxxx\r000000000000000000000000000000000000000000000xxxxx\r0000000000000000000000000000000000000000000000xxxx\rx0000000000000000000000000000000000000000000000xxx\rxx0000000000000000000000000000000000000000000000xx\rxxx0000000000000000000000000000000000000000000000x\rxxxx0000000000000000000000000000000000000000000000\rxxxxx000000000000000000000000000000000000000000000\rxxxxxx00000000000000000000000000000000000000000000\rxxxxxxx0000000000000000000000000000000000000000000\rxxxxxxxx000000000000000000000000000000000000000000\rxxxxxxxxx00000000000000000000000000000000000000000\rxxxxxxxxxx000000000000000000000000000000000000000x\rxxxxxxxxxxx0000000000000000000000000000000000000xx\rxxxxxxxxxxxx00000000000000000000000000000000000xxx\rxxxxxxxxxxxxx000000000000000000000000000000000xxxx\rxxxxxxxxxxxxxx0000000000000000000000000000000xxxxx\rxxxxxxxxxxxxxxx00000000000000000000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000000000000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000000000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx00000000000000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx000000000000000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx0000000000000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxx\r', 'h221 sw_tree2 22 1 0 1\r\nh204 sw_tree2 20 4 0 1\r\ng244 sw_tree1 24 4 0 1\r\ng275 sw_tree1 27 5 0 1\r\ni246 sw_tree3 24 6 0 1\r\nj278 sw_tree4 27 8 0 1\r\nj209 sw_tree4 20 9 0 1\r\nh319 sw_tree2 31 9 0 1\r\nh1610 sw_tree2 16 10 0 1\r\ni1013 sw_tree3 10 13 0 1\r\ni2713 sw_tree3 27 13 0 1\r\ng1815 sw_tree1 18 15 0 1\r\ni3916 sw_tree3 39 16 0 1\r\ng1017 sw_tree1 10 17 0 1\r\nh2517 sw_tree2 25 17 0 1\r\nh418 sw_tree2 4 18 0 1\r\nj718 sw_tree4 7 18 0 1\r\nj1519 sw_tree4 15 19 0 1\r\nj3520 sw_tree4 35 20 0 1\r\nj121 sw_tree4 1 21 0 1\r\ni1121 sw_tree3 11 21 0 1\r\ng522 sw_tree1 5 22 0 1\r\nh722 sw_tree2 7 22 0 1\r\ng2223 sw_tree1 22 23 0 1\r\nh4323 sw_tree2 43 23 0 1\r\nh425 sw_tree2 4 25 0 1\r\ng4725 sw_tree1 47 25 0 1\r\ni727 sw_tree3 7 27 0 1\r\ni2030 sw_tree3 20 30 0 1\r\nj3931 sw_tree4 39 31 0 1\r\ng1133 sw_tree1 11 33 0 1\r\ni4734 sw_tree3 47 34 100000 1\r\nj1636 sw_tree4 16 36 0 1\r\nh2937 sw_tree2 29 37 0 1\r\ni2239 sw_tree3 22 39 0 1\r\ni3843 sw_tree3 38 43 100000 1\r\ng2445 sw_tree1 24 45 0 1\r\nj3248 sw_tree4 32 48 0 1\r\nh2749 sw_tree2 27 49 0 1', null);
INSERT INTO `sslevels` VALUES ('95', '0', 'Polar Labyrinth', null, null, 'snowwar_arena_4 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx0000000000000000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000000000000000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxx0000000000000000000000000xxxxxxxxxxxxxxxx\rxxxxxxxx000000000000000000000000000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000000000000xxxxxxxxxxxxxx\rxxxxxx0000000000000000000000000000000xxxxxxxxxxxxx\rxxxxx000000000000000000000000000000000xxxxxxxxxxxx\rxxxx00000000000000000000000000000000000xxxxxxxxxxx\rxxx0000000000000000000000000000000000000xxxxxxxxxx\rxx000000000000000000000000000000000000000xxxxxxxxx\rx00000000000000000000000000000000000000000xxxxxxxx\r0000000000000000000000000000000000000000000xxxxxxx\r00000000000000000000000000000000000000000000xxxxxx\r000000000000000000000000000000000000000000000xxxxx\r0000000000000000000000000000000000000000000000xxxx\rx0000000000000000000000000000000000000000000000xxx\rxx0000000000000000000000000000000000000000000000xx\rxxx0000000000000000000000000000000000000000000000x\rxxxx0000000000000000000000000000000000000000000000\rxxxxx000000000000000000000000000000000000000000000\rxxxxxx00000000000000000000000000000000000000000000\rxxxxxxx0000000000000000000000000000000000000000000\rxxxxxxxx000000000000000000000000000000000000000000\rxxxxxxxxx00000000000000000000000000000000000000000\rxxxxxxxxxx000000000000000000000000000000000000000x\rxxxxxxxxxxx0000000000000000000000000000000000000xx\rxxxxxxxxxxxx00000000000000000000000000000000000xxx\rxxxxxxxxxxxxx000000000000000000000000000000000xxxx\rxxxxxxxxxxxxxx0000000000000000000000000000000xxxxx\rxxxxxxxxxxxxxxx00000000000000000000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000000000000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000000000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx00000000000000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx000000000000000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx0000000000000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx00000000000000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000000000000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxx\r', 'i231 sw_tree3 23 1 0 1\r\nh202 sw_tree2 20 2 0 1\r\nj254 sw_tree4 25 4 0 1\r\ng195 sw_tree1 19 5 0 1\r\na198 block_basic 19 8 0 1\r\na298 block_basic 29 8 0 1\r\na308 block_basic 30 8 0 1\r\na318 block_basic 31 8 0 1\r\na328 block_basic 32 8 100000 1\r\na338 block_basic 33 8 100000 1\r\na348 block_basic 34 8 100000 1\r\na358 block_basic 35 8 100000 1\r\na199 block_basic 19 9 0 1\r\na359 block_basic 35 9 100000 1\r\na1910 block_basic 19 10 0 1\r\na2210 block_basic 22 10 0 1\r\na2310 block_basic 23 10 0 1\r\na2410 block_basic 24 10 0 1\r\na2510 block_basic 25 10 0 1\r\na3510 block_basic 35 10 100000 1\r\na1011 block_basic 10 11 0 1\r\na1111 block_basic 11 11 0 1\r\na1211 block_basic 12 11 0 1\r\na1311 block_basic 13 11 0 1\r\na1411 block_basic 14 11 0 1\r\na1511 block_basic 15 11 0 1\r\na1611 block_basic 16 11 0 1\r\na1711 block_basic 17 11 0 1\r\na1811 block_basic 18 11 0 1\r\na1911 block_basic 19 11 0 1\r\na2211 block_basic 22 11 0 1\r\na2511 block_basic 25 11 0 1\r\na2811 block_basic 28 11 0 1\r\na2911 block_basic 29 11 0 1\r\na3011 block_basic 30 11 0 1\r\na3111 block_basic 31 11 0 1\r\na3211 block_basic 32 11 0 1\r\na3511 block_basic 35 11 100000 1\r\na1012 block_basic 10 12 0 1\r\na2212 block_basic 22 12 0 1\r\na2512 block_basic 25 12 0 1\r\na2812 block_basic 28 12 0 1\r\na3212 block_basic 32 12 0 1\r\na3512 block_basic 35 12 0 1\r\na1013 block_basic 10 13 0 1\r\na2213 block_basic 22 13 0 1\r\na2313 block_basic 23 13 0 1\r\na2413 block_basic 24 13 0 1\r\na2513 block_basic 25 13 0 1\r\na2813 block_basic 28 13 0 1\r\na3213 block_basic 32 13 0 1\r\na3513 block_basic 35 13 0 1\r\na1014 block_basic 10 14 0 1\r\na1314 block_basic 13 14 0 1\r\na1414 block_basic 14 14 0 1\r\na1514 block_basic 15 14 0 1\r\na1614 block_basic 16 14 0 1\r\na1914 block_basic 19 14 0 1\r\na2814 block_basic 28 14 0 1\r\na2914 block_basic 29 14 0 1\r\na3014 block_basic 30 14 0 1\r\na3114 block_basic 31 14 0 1\r\na3214 block_basic 32 14 0 1\r\na3514 block_basic 35 14 0 1\r\na1015 block_basic 10 15 0 1\r\na1315 block_basic 13 15 0 1\r\na1915 block_basic 19 15 0 1\r\na3515 block_basic 35 15 0 1\r\ng716 sw_tree1 7 16 0 1\r\na1016 block_basic 10 16 0 1\r\na1316 block_basic 13 16 0 1\r\na1916 block_basic 19 16 0 1\r\na2216 block_basic 22 16 0 1\r\na2516 block_basic 25 16 0 1\r\na3516 block_basic 35 16 0 1\r\na1017 block_basic 10 17 0 1\r\na1317 block_basic 13 17 0 1\r\na1617 block_basic 16 17 0 1\r\na1717 block_basic 17 17 0 1\r\na1817 block_basic 18 17 0 1\r\na1917 block_basic 19 17 0 1\r\na2217 block_basic 22 17 0 1\r\na2517 block_basic 25 17 0 1\r\na2817 block_basic 28 17 0 1\r\na2917 block_basic 29 17 0 1\r\na3017 block_basic 30 17 0 1\r\na3117 block_basic 31 17 0 1\r\na3217 block_basic 32 17 0 1\r\na3517 block_basic 35 17 0 1\r\na3617 block_basic 36 17 0 1\r\na3717 block_basic 37 17 0 1\r\na3817 block_basic 38 17 0 1\r\na3917 block_basic 39 17 0 1\r\na4017 block_basic 40 17 0 1\r\na4117 block_basic 41 17 100000 1\r\na4217 block_basic 42 17 100000 1\r\na1018 block_basic 10 18 0 1\r\na2218 block_basic 22 18 0 1\r\na2518 block_basic 25 18 0 1\r\na2818 block_basic 28 18 0 1\r\na4218 block_basic 42 18 100000 1\r\nh419 sw_tree2 4 19 0 1\r\na819 block_basic 8 19 0 1\r\na919 block_basic 9 19 0 1\r\na1019 block_basic 10 19 0 1\r\na2219 block_basic 22 19 0 1\r\na2519 block_basic 25 19 0 1\r\na2819 block_basic 28 19 0 1\r\na4219 block_basic 42 19 0 1\r\na1320 block_basic 13 20 0 1\r\na1620 block_basic 16 20 0 1\r\na1720 block_basic 17 20 0 1\r\na1820 block_basic 18 20 0 1\r\na1920 block_basic 19 20 0 1\r\na2220 block_basic 22 20 0 1\r\na2320 block_basic 23 20 0 1\r\na2420 block_basic 24 20 0 1\r\na2520 block_basic 25 20 0 1\r\na2820 block_basic 28 20 0 1\r\na2920 block_basic 29 20 0 1\r\na3020 block_basic 30 20 0 1\r\na3120 block_basic 31 20 0 1\r\na3220 block_basic 32 20 0 1\r\na3320 block_basic 33 20 0 1\r\na3620 block_basic 36 20 0 1\r\na3720 block_basic 37 20 0 1\r\na3820 block_basic 38 20 0 1\r\na3920 block_basic 39 20 0 1\r\na4220 block_basic 42 20 0 1\r\na1321 block_basic 13 21 0 1\r\na1621 block_basic 16 21 0 1\r\na1921 block_basic 19 21 0 1\r\na3321 block_basic 33 21 0 1\r\na3621 block_basic 36 21 0 1\r\na4221 block_basic 42 21 0 1\r\na822 block_basic 8 22 0 1\r\na922 block_basic 9 22 0 1\r\na1022 block_basic 10 22 0 1\r\na1322 block_basic 13 22 0 1\r\na1922 block_basic 19 22 0 1\r\na3322 block_basic 33 22 0 1\r\na3622 block_basic 36 22 0 1\r\na4222 block_basic 42 22 0 1\r\na1023 block_basic 10 23 0 1\r\na1323 block_basic 13 23 0 1\r\na1923 block_basic 19 23 0 1\r\na3323 block_basic 33 23 0 1\r\na3623 block_basic 36 23 0 1\r\na3923 block_basic 39 23 0 1\r\na4023 block_basic 40 23 0 1\r\na4123 block_basic 41 23 0 1\r\na4223 block_basic 42 23 0 1\r\ni324 sw_tree3 3 24 0 1\r\na1024 block_basic 10 24 0 1\r\na1324 block_basic 13 24 0 1\r\na1624 block_basic 16 24 0 1\r\na1724 block_basic 17 24 0 1\r\na1824 block_basic 18 24 0 1\r\na1924 block_basic 19 24 0 1\r\na3324 block_basic 33 24 0 1\r\na3624 block_basic 36 24 0 1\r\na3924 block_basic 39 24 0 1\r\na4024 block_basic 40 24 0 1\r\na4124 block_basic 41 24 0 1\r\na4224 block_basic 42 24 0 1\r\na825 block_basic 8 25 0 1\r\na925 block_basic 9 25 0 1\r\na1025 block_basic 10 25 0 1\r\nh4926 sw_tree2 49 26 0 1\r\na1327 block_basic 13 27 0 1\r\na1627 block_basic 16 27 0 1\r\na1727 block_basic 17 27 0 1\r\na1827 block_basic 18 27 0 1\r\na1927 block_basic 19 27 0 1\r\na3327 block_basic 33 27 0 1\r\na3427 block_basic 34 27 0 1\r\na3527 block_basic 35 27 0 0\r\na3627 block_basic 36 27 0 1\r\na3927 block_basic 39 27 0 1\r\na4027 block_basic 40 27 0 1\r\na4127 block_basic 41 27 0 1\r\na4227 block_basic 42 27 0 1\r\na4327 block_basic 43 27 0 1\r\na4427 block_basic 44 27 0 1\r\na828 block_basic 8 28 0 1\r\na928 block_basic 9 28 0 1\r\na1028 block_basic 10 28 0 1\r\na1328 block_basic 13 28 0 1\r\na1928 block_basic 19 28 0 1\r\na3328 block_basic 33 28 0 1\r\na1029 block_basic 10 29 0 1\r\na1329 block_basic 13 29 0 1\r\na1929 block_basic 19 29 0 1\r\na3329 block_basic 33 29 0 1\r\na1030 block_basic 10 30 0 1\r\na1330 block_basic 13 30 0 1\r\na1630 block_basic 16 30 0 1\r\na1930 block_basic 19 30 0 1\r\na3330 block_basic 33 30 0 1\r\na3430 block_basic 34 30 0 1\r\na3530 block_basic 35 30 0 1\r\na3630 block_basic 36 30 0 1\r\na3930 block_basic 39 30 0 1\r\na4030 block_basic 40 30 0 1\r\na4130 block_basic 41 30 0 1\r\na4230 block_basic 42 30 0 1\r\na4330 block_basic 43 30 0 1\r\na4430 block_basic 44 30 0 1\r\na1031 block_basic 10 31 0 1\r\na1331 block_basic 13 31 0 1\r\na1631 block_basic 16 31 0 1\r\na1931 block_basic 19 31 0 1\r\na1032 block_basic 10 32 0 1\r\na1332 block_basic 13 32 0 1\r\na1632 block_basic 16 32 0 1\r\na1932 block_basic 19 32 0 1\r\na1033 block_basic 10 33 100000 1\r\na1333 block_basic 13 33 0 1\r\na1433 block_basic 14 33 0 1\r\na1533 block_basic 15 33 0 1\r\na1633 block_basic 16 33 0 1\r\na1933 block_basic 19 33 0 1\r\na2233 block_basic 22 33 0 1\r\na2333 block_basic 23 33 0 1\r\na2433 block_basic 24 33 0 1\r\na2533 block_basic 25 33 0 1\r\na2633 block_basic 26 33 0 1\r\na2933 block_basic 29 33 0 1\r\na3033 block_basic 30 33 0 1\r\na3133 block_basic 31 33 0 1\r\na3233 block_basic 32 33 0 1\r\na3333 block_basic 33 33 0 1\r\na3433 block_basic 34 33 0 1\r\na3533 block_basic 35 33 0 1\r\na3833 block_basic 38 33 0 1\r\na3933 block_basic 39 33 0 1\r\na4033 block_basic 40 33 0 1\r\na4133 block_basic 41 33 0 1\r\na4233 block_basic 42 33 0 1\r\na4333 block_basic 43 33 0 1\r\na1934 block_basic 19 34 0 1\r\na2234 block_basic 22 34 0 1\r\na2634 block_basic 26 34 0 1\r\na2934 block_basic 29 34 0 1\r\na3834 block_basic 38 34 0 1\r\na4334 block_basic 43 34 0 1\r\na1935 block_basic 19 35 0 1\r\na2235 block_basic 22 35 0 1\r\na2635 block_basic 26 35 0 1\r\na2935 block_basic 29 35 0 1\r\na3835 block_basic 38 35 0 1\r\na3935 block_basic 39 35 0 1\r\na4035 block_basic 40 35 0 1\r\na4135 block_basic 41 35 0 1\r\na4235 block_basic 42 35 0 1\r\na4335 block_basic 43 35 0 1\r\na1636 block_basic 16 36 0 1\r\na1736 block_basic 17 36 0 1\r\na1836 block_basic 18 36 0 1\r\na1936 block_basic 19 36 0 1\r\na2236 block_basic 22 36 0 1\r\na2636 block_basic 26 36 0 1\r\na2936 block_basic 29 36 0 1\r\na3236 block_basic 32 36 0 1\r\na3336 block_basic 33 36 0 1\r\na3436 block_basic 34 36 0 1\r\na3536 block_basic 35 36 0 1\r\na2237 block_basic 22 37 0 1\r\na2337 block_basic 23 37 0 1\r\na2437 block_basic 24 37 0 1\r\na2537 block_basic 25 37 0 1\r\na2637 block_basic 26 37 0 1\r\na2937 block_basic 29 37 0 1\r\na3237 block_basic 32 37 0 1\r\na3337 block_basic 33 37 0 1\r\na3437 block_basic 34 37 0 1\r\na3537 block_basic 35 37 0 1\r\na3838 block_basic 38 38 0 1\r\na3938 block_basic 39 38 0 1\r\na4038 block_basic 40 38 0 1\r\na4138 block_basic 41 38 0 1\r\na1639 block_basic 16 39 100000 1\r\na1739 block_basic 17 39 0 1\r\na1839 block_basic 18 39 0 1\r\na1939 block_basic 19 39 0 1\r\na3839 block_basic 38 39 0 1\r\na4139 block_basic 41 39 0 1\r\na1940 block_basic 19 40 0 1\r\na2240 block_basic 22 40 0 1\r\na2340 block_basic 23 40 0 1\r\na2440 block_basic 24 40 0 1\r\na2540 block_basic 25 40 0 1\r\na2640 block_basic 26 40 0 1\r\na2940 block_basic 29 40 0 1\r\na3040 block_basic 30 40 0 1\r\na3140 block_basic 31 40 0 1\r\na3240 block_basic 32 40 0 1\r\na3540 block_basic 35 40 0 1\r\na3640 block_basic 36 40 0 1\r\na3740 block_basic 37 40 0 1\r\na3840 block_basic 38 40 0 1\r\na4140 block_basic 41 40 100000 1\r\na1941 block_basic 19 41 0 1\r\na2241 block_basic 22 41 0 1\r\na2641 block_basic 26 41 0 1\r\na2941 block_basic 29 41 0 1\r\na3241 block_basic 32 41 0 1\r\na4141 block_basic 41 41 100000 1\r\na1942 block_basic 19 42 100000 1\r\na2242 block_basic 22 42 0 1\r\na2342 block_basic 23 42 0 1\r\na2442 block_basic 24 42 0 1\r\na2542 block_basic 25 42 0 1\r\na2642 block_basic 26 42 0 1\r\na2942 block_basic 29 42 0 1\r\na3242 block_basic 32 42 0 1\r\na4142 block_basic 41 42 100000 1\r\na2943 block_basic 29 43 0 1\r\na3243 block_basic 32 43 0 1\r\na3543 block_basic 35 43 0 1\r\na3643 block_basic 36 43 0 1\r\na3743 block_basic 37 43 0 1\r\na3843 block_basic 38 43 100000 1\r\na3943 block_basic 39 43 100000 1\r\na4043 block_basic 40 43 100000 1\r\na4143 block_basic 41 43 100000 1\r\na2944 block_basic 29 44 0 1\r\na3244 block_basic 32 44 0 1\r\na3544 block_basic 35 44 0 1\r\na2945 block_basic 29 45 0 1\r\na3045 block_basic 30 45 0 1\r\na3145 block_basic 31 45 0 1\r\na3245 block_basic 32 45 0 1\r\na3545 block_basic 35 45 0 1\r\na3546 block_basic 35 46 100000 1', null);
INSERT INTO `sslevels` VALUES ('96', '0', 'Riverbank Siege', null, null, 'snowwar_arena_5 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxx00000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxx0000000xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxx000000000xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx000000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx00000000000000xxxx0xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxx0000000000000000xxx00xxxxxxxxxxxxxxxxxx\rxxxxxxxxxx00000000000000000xxx0000xxxxxxxxxxxxxxxx\rxxxxxxxxx000000000000000000xxx00000xxxxxxxxxxxxxxx\rxxxxxxxx0000000000000000000xxx00000xxxxxxxxxxxxxxx\rxxxxxxx00000000000000000000xxx000000xxxxxxxxxxxxxx\rxxxxxx000000000000000000000xxx0000000xxxxxxxxxxxxx\rxxxxx0000000000000000000000xxx00000000xxxxxxxxxxxx\rxxxx00000000000000000000000xxx000000000xxxxxxxxxxx\rxxx000000000000000000000000xxx0000000000xxxxxxxxxx\rxx0000000000000000000000000xxx00000000000xxxxxxxxx\rx00000000000000000000000000xxx000000000000xxxxxxxx\r000000000000000000000000000xxx0000000000000xxxxxxx\r000000000000000000000000000xxx00000000000000xxxxxx\r000000000000000000000000000xxx000000000000000xxxxx\r0000000000000000000000000000000000000000000000xxxx\rx0000000000000000000000000000000000000000000000xxx\rxx00000000000000000000000000x0000000000000000000xx\rxxx000000000000000000000000xxx0000000000000000000x\rxxxx00000000000000000000000x0000000000000000000000\rxxxxx000000000000000000000000000000000000000000000\rxxxxxx00000000000000000000000000000000000000000000\rxxxxxxx00000000000000000000xxx00000000000000000000\rxxxxxxxx0000000000000000000xxx00000000000000000000\rxxxxxxxxx000000000000000000xxx00000000000000000000\rxxxxxxxxxx00000000000000000xxx0000000000000000000x\rxxxxxxxxxxx0000000000000000xxx000000000000000000xx\rxxxxxxxxxxxx000000000000000xxx00000000000000000xxx\rxxxxxxxxxxxxx00000000000000xxx0000000000000000xxxx\rxxxxxxxxxxxxxx0000000000000xxx000000000000000xxxxx\rxxxxxxxxxxxxxxx000000000000xxx00000000000000xxxxxx\rxxxxxxxxxxxxxxxx00000000000xxx0000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000xxx000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx000000000xxx00000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx00000000xxx0000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx0000000xxx000000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx000000xxx00000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx00000xxx0000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000xxx000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx000xxx00000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx00xxx0000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx0xxx000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r', 'c230 block_basic3 23 0 0 1\r\nb231 block_basic2 23 1 0 1\r\nc232 block_basic3 23 2 0 1\r\nb233 block_basic2 23 3 0 1\r\ng184 sw_tree1 18 4 0 1\r\nj204 sw_tree4 20 4 0 1\r\nc234 block_basic3 23 4 0 1\r\nh264 sw_tree2 26 4 0 1\r\nb235 block_basic2 23 5 0 1\r\nj255 sw_tree4 25 5 0 1\r\nh186 sw_tree2 18 6 0 1\r\ni226 sw_tree3 22 6 0 1\r\nc236 block_basic3 23 6 0 1\r\nb237 block_basic2 23 7 0 1\r\nc118 block_basic3 11 8 0 1\r\nb128 block_basic2 12 8 0 1\r\nc138 block_basic3 13 8 0 1\r\nb148 block_basic2 14 8 0 1\r\nc158 block_basic3 15 8 0 1\r\nb168 block_basic2 16 8 0 1\r\nc178 block_basic3 17 8 0 1\r\nb188 block_basic2 18 8 0 1\r\nc198 block_basic3 19 8 0 1\r\nb208 block_basic2 20 8 0 1\r\nc218 block_basic3 21 8 0 1\r\nb228 block_basic2 22 8 0 1\r\no238 block_arch1b 23 8 0 1\r\ng268 sw_tree1 26 8 0 1\r\nc119 block_basic3 11 9 0 1\r\nb1110 block_basic2 11 10 0 1\r\nu2310 block_arch3b 23 10 0 1\r\nc811 block_basic3 8 11 0 1\r\nb911 block_basic2 9 11 0 1\r\nb1011 block_basic2 10 11 0 1\r\na1111 block_basic 11 11 0 1\r\na1411 block_basic 14 11 0 1\r\nb1511 block_basic2 15 11 0 1\r\na1611 block_basic 16 11 0 1\r\nb1711 block_basic2 17 11 0 1\r\na1811 block_basic 18 11 0 1\r\nq1911 block_ice2 19 11 0 1\r\np2011 block_ice 20 11 0 1\r\na2211 block_basic 22 11 0 1\r\nb2311 block_basic2 23 11 0 1\r\nb812 block_basic2 8 12 0 1\r\nb1912 block_basic2 19 12 0 1\r\na2312 block_basic 23 12 0 1\r\nc813 block_basic3 8 13 0 1\r\nb1913 block_basic2 19 13 0 1\r\nb2313 block_basic2 23 13 0 1\r\nb814 block_basic2 8 14 0 1\r\na1214 block_basic 12 14 0 1\r\no1914 block_arch1b 19 14 0 1\r\na2314 block_basic 23 14 0 1\r\nc815 block_basic3 8 15 0 1\r\nb1215 block_basic2 12 15 0 1\r\nb2315 block_basic2 23 15 0 1\r\np3415 block_ice 34 15 0 1\r\nb816 block_basic2 8 16 0 1\r\nb1216 block_basic2 12 16 0 1\r\nu1916 block_arch3b 19 16 0 1\r\na2316 block_basic 23 16 0 1\r\nq3416 block_ice2 34 16 0 1\r\nc817 block_basic3 8 17 0 1\r\nb1217 block_basic2 12 17 0 1\r\nb1917 block_basic2 19 17 0 1\r\np3417 block_ice 34 17 0 1\r\ni418 sw_tree3 4 18 0 1\r\nb818 block_basic2 8 18 0 1\r\na1018 block_basic 10 18 0 1\r\na1218 block_basic 12 18 0 1\r\na1418 block_basic 14 18 0 1\r\nb1518 block_basic2 15 18 0 1\r\na1618 block_basic 16 18 0 1\r\nb1718 block_basic2 17 18 0 1\r\na1818 block_basic 18 18 0 1\r\nq1918 block_ice2 19 18 0 1\r\np2018 block_ice 20 18 0 1\r\nq3418 block_ice2 34 18 0 1\r\nr019 sw_backround6 0 19 0 0\r\nc819 block_basic3 8 19 0 1\r\nb1919 block_basic2 19 19 0 1\r\np3419 block_ice 34 19 0 1\r\nb820 block_basic2 8 20 0 1\r\no1920 block_arch1b 19 20 0 1\r\nc821 block_basic3 8 21 0 1\r\nh622 sw_tree2 6 22 0 1\r\nb822 block_basic2 8 22 0 1\r\nu1922 block_arch3b 19 22 0 1\r\ng423 sw_tree1 4 23 0 1\r\nc823 block_basic3 8 23 0 1\r\nb1923 block_basic2 19 23 0 1\r\nb824 block_basic2 8 24 0 1\r\nb1924 block_basic2 19 24 0 1\r\nj425 sw_tree4 4 25 0 1\r\nc825 block_basic3 8 25 0 1\r\nq925 block_ice2 9 25 0 1\r\np1025 block_ice 10 25 0 1\r\np1325 block_ice 13 25 0 1\r\nq1425 block_ice2 14 25 0 1\r\na1525 block_basic 15 25 0 1\r\nb1625 block_basic2 16 25 0 1\r\na1725 block_basic 17 25 0 1\r\nb1825 block_basic2 18 25 0 1\r\nc1925 block_basic3 19 25 0 1\r\nq2025 block_ice2 20 25 0 1\r\np2125 block_ice 21 25 0 1\r\nb826 block_basic2 8 26 0 1\r\nb1926 block_basic2 19 26 0 1\r\np4026 block_ice 40 26 0 1\r\nc827 block_basic3 8 27 0 1\r\nb1927 block_basic2 19 27 0 1\r\nq4027 block_ice2 40 27 0 1\r\nb828 block_basic2 8 28 0 1\r\no1928 block_arch1b 19 28 0 1\r\np4028 block_ice 40 28 0 1\r\nc829 block_basic3 8 29 0 1\r\nq4029 block_ice2 40 29 0 1\r\nb830 block_basic2 8 30 0 1\r\nu1930 block_arch3b 19 30 0 1\r\np4030 block_ice 40 30 0 1\r\nb1931 block_basic2 19 31 0 1\r\na1032 block_basic 10 32 0 1\r\na1232 block_basic 12 32 0 1\r\na1432 block_basic 14 32 0 1\r\nb1532 block_basic2 15 32 0 1\r\na1632 block_basic 16 32 0 1\r\nb1732 block_basic2 17 32 0 1\r\na1832 block_basic 18 32 0 1\r\nq1932 block_ice2 19 32 0 1\r\np2032 block_ice 20 32 0 1\r\nb1233 block_basic2 12 33 0 1\r\nb1933 block_basic2 19 33 0 1\r\nb1234 block_basic2 12 34 0 1\r\no1934 block_arch1b 19 34 0 1\r\na2335 block_basic 23 35 0 1\r\np3435 block_ice 34 35 0 1\r\nu1936 block_arch3b 19 36 0 1\r\nb2336 block_basic2 23 36 0 1\r\nq3436 block_ice2 34 36 0 1\r\nb1937 block_basic2 19 37 0 1\r\na2337 block_basic 23 37 0 1\r\np3437 block_ice 34 37 0 1\r\nb1938 block_basic2 19 38 0 1\r\nb2338 block_basic2 23 38 0 1\r\nq3438 block_ice2 34 38 0 1\r\nb1739 block_basic2 17 39 0 1\r\na1839 block_basic 18 39 0 1\r\nq1939 block_ice2 19 39 0 1\r\np2039 block_ice 20 39 0 1\r\na2239 block_basic 22 39 0 1\r\na2339 block_basic 23 39 0 1\r\np3439 block_ice 34 39 0 1\r\na2340 block_basic 23 40 0 1\r\na2341 block_basic 23 41 0 1\r\ng2143 sw_tree1 21 43 0 1\r\na2343 block_basic 23 43 0 1\r\nb2344 block_basic2 23 44 0 1\r\ni3144 sw_tree3 31 44 0 1\r\nc2345 block_basic3 23 45 0 1\r\ng3046 sw_tree1 30 46 0 1\r\nj3246 sw_tree4 32 46 0 1\r\nh3148 sw_tree2 31 48 0 1', null);
INSERT INTO `sslevels` VALUES ('97', '0', 'Skull Falls Assault', null, null, 'snowwar_arena_6 15805', '0', '30', 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx0xxxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00xxxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx000xxxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx0000xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxx0xxxxxxx00000xxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxx00xxxxxxx000000xxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxx000xxxxxxx0000000xxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxx0000xxxxxxx00000000xxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxx00000xxxxxxx000000000xxxxxxxxxxxxxxxxx\rxxxxxxxxxxx000000xxxxxxx0000000000xxxxxxxxxxxxxxxx\rxxxxxxxxxx0000000xxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxx00000000xxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxx000000000xxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxx0000000000xxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxx00000000000xxxxxxx00000000000xxx0xxxxxxxxxxx\rxxxxx000000000000xxxxxxx00000000000xxx00xxxxxxxxxx\rxxxx0000000000000xxxxxxx00000000000xxx000xxxxxxxxx\rxxx00000000000000000000000000000000xxx0000xxxxxxxx\r0x00000000000000000000000000000000000000000xxxxxxx\rx0000000000000000xxx0xxx00000000000xxx000000xxxxxx\r00000000000000000xx000xx00000000000xxx0000000xxxxx\r00000000000000000000x00000000000000xxx00000000xxxx\rx000000000000000000xxx0000000000000xxx000000000xxx\rxx000000000000000000000000000000000xxx0000000000xx\rxxx00000000000000000000000000000000xxx00000000000x\rxxxx000000000000000x0x0000000000000xxx000000000000\rxxxxx00000000000000xxx0000000000000xxx000000000000\rxxxxxx0000000000000xxx0000000xxx0xxxxx000000000000\rxxxxxxx000000000000xxx000000xxxx0xxxxx000000000000\rxxxxxxxx00000000000xxx000000xxxx0xxxx0000000000000\rxxxxxxxxx0000000000xxx000000xxx0000000000000000000\rxxxxxxxxxx000000000xxx000000xxx000000000000000000x\rxxxxxxxxxxx00000000xxx000000xxx00000000000000000xx\rxxxxxxxxxxxx0000000xxx000000xxx0000000000000000xxx\rxxxxxxxxxxxxx000000xxxxxxxxxxxx000000000000000xxxx\rxxxxxxxxxxxxxx00000xxxxxxxxxxxx00000000000000xxxxx\rxxxxxxxxxxxxxxx00000xxxxxxxxxxx0000000000000xxxxxx\rxxxxxxxxxxxxxxxx000000000000xxx000000000000xxxxxxx\rxxxxxxxxxxxxxxxxx0000000000000000000000000xxxxxxxx\rxxxxxxxxxxxxxxxxxx00000000000000000000000xxxxxxxxx\rxxxxxxxxxxxxxxxxxxx0000000000x0000000000xxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxx00000000xxx00000000xxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxx0000000xxx0000000xxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxx000000x00000000xxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxx0000000000000xxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxx00000000000xxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxx000xx0000xxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxx00xxxxxxxxxxxxxxxxxxxxxx\rxxxxxxxxxxxxxxxxxxxxxxxxxxx0xxxxxxxxxxxxxxxxxxxxxx\r', 'i299 sw_tree3 29 9 0 1\r\nh1410 sw_tree2 14 10 0 1\r\nj1214 sw_tree4 12 14 0 1\r\ni1515 sw_tree3 15 15 0 1\r\ns019 sw_backround7 0 19 0 0\r\na4219 block_basic 42 19 0 1\r\na4220 block_basic 42 20 0 1\r\na4221 block_basic 42 21 0 1\r\ng2922 sw_tree1 29 22 0 1\r\na4222 block_basic 42 22 0 1\r\nj3223 sw_tree4 32 23 0 1\r\np4225 block_ice 42 25 0 1\r\na4325 block_basic 43 25 0 1\r\np4625 block_ice 46 25 0 1\r\na4725 block_basic 47 25 0 1\r\np4825 block_ice 48 25 0 1\r\nh2926 sw_tree2 29 26 0 1\r\nb4226 block_basic2 42 26 0 1\r\nq4227 block_ice2 42 27 0 1\r\nb4228 block_basic2 42 28 0 1\r\np4528 block_ice 45 28 0 1\r\na4628 block_basic 46 28 0 1\r\nq4229 block_ice2 42 29 0 1\r\na4529 block_basic 45 29 0 1\r\ni1330 sw_tree3 13 30 0 1\r\na4230 block_basic 42 30 0 1\r\ng1632 sw_tree1 16 32 0 1\r\nh1233 sw_tree2 12 33 0 1\r\na3733 block_basic 37 33 0 1\r\nq4233 block_ice2 42 33 0 1\r\nq3734 block_ice2 37 34 0 1\r\na4234 block_basic 42 34 0 1\r\nc3735 block_basic3 37 35 0 1\r\np4235 block_ice 42 35 0 1\r\nq3736 block_ice2 37 36 0 1\r\nb4236 block_basic2 42 36 0 1\r\np4336 block_ice 43 36 0 1\r\na4436 block_basic 44 36 0 1\r\np4536 block_ice 45 36 100000 1\r\na3737 block_basic 37 37 0 1\r\np3538 block_ice 35 38 0 1\r\np3638 block_ice 36 38 0 1\r\np3738 block_ice 37 38 0 1\r\ni2540 sw_tree3 25 40 0 1\r\np3540 block_ice 35 40 0 1\r\np3640 block_ice 36 40 0 1\r\np3740 block_ice 37 40 0 1\r\np3741 block_ice 37 41 0 1\r\np3742 block_ice 37 42 0 1\r\nj2643 sw_tree4 26 43 0 1\r\np3743 block_ice 37 43 0 1', null);
INSERT INTO `sslevels` VALUES ('17', '3', 'BattleBall lobby', 'hh_game_bb,hh_game_bb_room,hh_game_bb_ui,hh_gamesys', 'bb_lobby_beginner_2', 'bb_lobby_1 15222', '1', '30', 'xxx2222222222222222x\rxxx2222222222222222x\rxxx2222222222222222x\rxxx2222222222222222x\rxxx11111111111111111\r11x11111111111111111\r11x11111111111111111\r11x11111111111111111\rx1x11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxx11111111111111111\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxx00000000000\rxxxxxxxxxxxxx000xxxx\rxxxxxxxxxxxxx000xxxx\rxxxxxxxxxxxxx000xxxx\r', 'n30 bb_crossrd 3 0 2 2 1\r\nb40 bb_bench1 4 0 2 4 2\r\nc50 bb_bench2 5 0 2 4 2\r\nh80 bb_plant1 8 0 2 0 1\r\nd90 bb_sofa1 9 0 2 4 2\r\ne100 bb_sofa2 10 0 2 4 2\r\nh110 bb_plant1 11 0 2 0 1\r\nd120 bb_sofa1 12 0 2 4 2\r\ne130 bb_sofa2 13 0 2 4 2\r\nh140 bb_plant1 14 0 2 0 1\r\nb160 bb_bench1 16 0 2 4 2\r\nc170 bb_bench2 17 0 2 4 2\r\nk180 bb_corner1out 18 0 2 0 1\r\nb31 bb_bench1 3 1 2 2 2\r\no181 bb_wallend1in 18 1 2 2 1\r\nc32 bb_bench2 3 2 2 2 2\r\no182 bb_wallend1in 18 2 2 2 1\r\nj33 bb_plant3 3 3 2 0 1\r\nt73 bb_special 7 3 2 6 1\r\no83 bb_wallend1in 8 3 2 6 1\r\no93 bb_wallend1in 9 3 2 6 1\r\no103 bb_wallend1in 10 3 2 6 1\r\no113 bb_wallend1in 11 3 2 6 1\r\nn123 bb_crossrd 12 3 2 2 1\r\nn163 bb_crossrd 16 3 2 0 1\r\no173 bb_wallend1in 17 3 2 6 1\r\nn183 bb_crossrd 18 3 2 2 1\r\np34 bb_wallend2in 3 4 1 4 1\r\no74 bb_wallend1in 7 4 1 4 1\r\nb84 bb_bench1 8 4 1 4 2\r\nc94 bb_bench2 9 4 1 4 2\r\nb104 bb_bench1 10 4 1 4 2\r\nc114 bb_bench2 11 4 1 4 2\r\np124 bb_wallend2in 12 4 1 4 1\r\no164 bb_wallend1in 16 4 1 4 1\r\nb174 bb_bench1 17 4 1 4 2\r\nc184 bb_bench2 18 4 1 4 2\r\nm194 bb_wallendout 19 4 1 4 1\r\na75 bb_stool 7 5 1 2 2\r\na125 bb_stool 12 5 1 6 2\r\nb195 bb_bench1 19 5 1 6 2\r\na36 bb_stool 3 6 1 6 2\r\nc196 bb_bench2 19 6 1 6 2\r\nf97 bb_chair 9 7 1 4 2\r\nf107 bb_chair 10 7 1 4 2\r\nb177 bb_bench1 17 7 1 0 2\r\nc187 bb_bench2 18 7 1 0 2\r\nm197 bb_wallendout 19 7 1 0 1\r\na38 bb_stool 3 8 1 6 2\r\nu178 bb_extra 17 8 1 4 1\r\nu188 bb_extra 18 8 1 2 1\r\nn198 bb_crossrd 19 8 1 6 1\r\na39 bb_stool 3 9 1 6 2\r\nf99 bb_chair 9 9 1 0 2\r\nf109 bb_chair 10 9 1 0 2\r\nb179 bb_bench1 17 9 1 4 2\r\nc189 bb_bench2 18 9 1 4 2\r\nm199 bb_wallendout 19 9 1 4 1\r\nb1910 bb_bench1 19 10 1 6 2\r\na711 bb_stool 7 11 1 2 2\r\na1211 bb_stool 12 11 1 6 2\r\nc1911 bb_bench2 19 11 1 6 2\r\no712 bb_wallend1in 7 12 1 0 1\r\nb812 bb_bench1 8 12 1 0 2\r\nc912 bb_bench2 9 12 1 0 2\r\nb1012 bb_bench1 10 12 1 0 2\r\nc1112 bb_bench2 11 12 1 0 2\r\np1212 bb_wallend2in 12 12 1 0 1\r\nb1712 bb_bench1 17 12 1 0 2\r\nc1812 bb_bench2 18 12 1 0 2\r\nm1912 bb_wallendout 19 12 1 0 1\r\nk713 bb_corner1out 7 13 1 4 1\r\nl813 bb_wallout 8 13 1 6 1\r\nl913 bb_wallout 9 13 1 6 1\r\nl1013 bb_wallout 10 13 1 6 1\r\nl1113 bb_wallout 11 13 1 6 1\r\nt1213 bb_special 12 13 1 2 1\r\nm1613 bb_wallendout 16 13 1 6 1\r\nl1713 bb_wallout 17 13 1 6 1\r\nl1813 bb_wallout 18 13 1 6 1\r\nk1913 bb_corner1out 19 13 1 2 1\r\ng914 bb_plant0 9 14 0 6 1\r\nd1014 bb_sofa1 10 14 0 4 2\r\ne1114 bb_sofa2 11 14 0 4 2\r\ni1214 bb_plant2 12 14 0 0 1\r\ni1614 bb_plant2 16 14 0 6 1\r\nd1714 bb_sofa1 17 14 0 4 2\r\ne1814 bb_sofa2 18 14 0 4 2\r\ng1914 bb_plant0 19 14 0 0 1\r\nd915 bb_sofa1 9 15 0 2 2\r\nd1915 bb_sofa1 19 15 0 6 2\r\ne916 bb_sofa2 9 16 0 2 2\r\ne1916 bb_sofa2 19 16 0 6 2\r\nd917 bb_sofa1 9 17 0 2 2\r\nd1917 bb_sofa1 19 17 0 6 2\r\ne918 bb_sofa2 9 18 0 2 2\r\ne1918 bb_sofa2 19 18 0 6 2\r\ng919 bb_plant0 9 19 0 4 1\r\nd1019 bb_sofa1 10 19 0 0 2\r\ne1119 bb_sofa2 11 19 0 0 2\r\nj1219 bb_plant3 12 19 0 2 1\r\nj1619 bb_plant3 16 19 0 4 1\r\nd1719 bb_sofa1 17 19 0 0 2\r\ne1819 bb_sofa2 18 19 0 0 2\r\ng1919 bb_plant0 19 19 0 2 1', '14 19 0');
INSERT INTO `system` VALUES ('1', '');
INSERT INTO `system_config` VALUES ('game_port', '21');
INSERT INTO `system_config` VALUES ('lang', 'en');
INSERT INTO `system_config` VALUES ('game_maxconnections', '400');
INSERT INTO `system_config` VALUES ('mus_port', '22');
INSERT INTO `system_config` VALUES ('mus_maxconnections', '100');
INSERT INTO `system_config` VALUES ('mus_host', '127.0.0.1');
INSERT INTO `system_config` VALUES ('welcomemessage_enable', '1');
INSERT INTO `system_config` VALUES ('welcomemessage_text', 'Holograph Emulator! :O');
INSERT INTO `system_config` VALUES ('wordfilter_enable', '1');
INSERT INTO `system_config` VALUES ('wordfilter_censor', 'bobba');
INSERT INTO `system_config` VALUES ('chatanims_enable', '1');
INSERT INTO `system_config` VALUES ('trading_enable', '1');
INSERT INTO `system_config` VALUES ('recycler_enable', '1');
INSERT INTO `system_config` VALUES ('recycler_minownertime', '43200');
INSERT INTO `system_config` VALUES ('recycler_session_length', '60');
INSERT INTO `system_config` VALUES ('recycler_session_expirelength', '10080');
INSERT INTO `system_recycler` VALUES ('20', '419');
INSERT INTO `system_recycler` VALUES ('30', '420');
INSERT INTO `system_recycler` VALUES ('40', '421');
INSERT INTO `system_strings` VALUES ('console_onhotelview', 'On Hotel View', 'auf Hotelansicht');
INSERT INTO `system_strings` VALUES ('modtool_accesserror', 'Sorry, but you do not have access to this feature of the MOD-Tool.\\rIf you think you should have access to this feature, then please contact the Hotel staff.\\rIf not, gtfo.', null);
INSERT INTO `system_strings` VALUES ('modtool_actionfail', 'Action failed.', null);
INSERT INTO `system_strings` VALUES ('modtool_rankerror', 'You do not have the rights for this action on this user!', null);
INSERT INTO `system_strings` VALUES ('modtool_usernotfound', 'Probably the user is offline or does not exist.', null);
INSERT INTO `system_strings` VALUES ('room_rightsreset', 'The roomowner has reset all the roomrights.<br>Please re-enter the room.', null);
INSERT INTO `system_strings` VALUES ('trading_disabled', 'Sorry, but the Hotel staff has disabled trading.\\rPlease try later!', null);
INSERT INTO `system_strings` VALUES ('trading_nottradeable', 'Sorry, but you can\'t trade this item!', null);
INSERT INTO `users` VALUES ('1', 'Meth0d', 'deamon123', '7', 'private', '1-1-2008', '16-02-2008', 'hd-190-1.ch-260-62.lg-280-110.sh-295-91.hr-802-31.fa-1201-', 'M', 's0beit', '', '9297', '0', '1,XM8', '25-3-2008 18:35:34', null, '1', null, null);
INSERT INTO `users` VALUES ('2', 'Nillus', 'rofl10', '7', 'private', '26-3-1992', '16-02-2008', 'hr-115-45.hd-180-1.ch-255-62.lg-280-62.sh-908-62.wa-2011-62.ha-1023-62.he-1601-62.fa-1204-62', 'M', 'Yaeh =]', 'Meh', '7239', '35', '1,ADM', '16-4-2008 20:49:52', null, '0', null, '86.81.163.182');
INSERT INTO `users` VALUES ('10', 'NGangsta', 'jeaapishomo', '1', 'ieeelll@hotmail.com', '1-3-1900', '22-02-2008', 'hr-170-32.hd-180-22.ch-210-62.lg-270-62.sh-905-62.ha-1008-', 'M', 'Hjb man! Koneinen FTW !', 'Yeah! :D', '0', '44', null, '30-3-2008 21:30:47', null, '0', null, '86.81.163.182');
INSERT INTO `users` VALUES ('11', 'Qurb', 'bob123', '1', 'private@live.com', '8-11-1991', '27-03-2008', 'hr-681-45.hd-180-8.ch-262-65.lg-285-67.sh-906-85.ea-1403-88.wa-2004-69', 'M', '', '', '0', '0', '0,', '15-04-2008 15:26:07', null, '0', null, '58.168.239.204');
INSERT INTO `users` VALUES ('12', 'FloWe', 'test123', '1', 'FloWe1902@aol.com', '19-2-1994', '28-03-2008', 'hr-155-45.hd-180-2.ch-260-62.lg-285-64.sh-906-86.ea-1406-70.ca-1804-62.fa-1205-62.ha-1014-64', 'M', '', '', '0', '0', '0,', '01-04-2008 18:21:06', null, '0', null, '78.53.55.160');
INSERT INTO `users` VALUES ('13', 'Daney', '123123', '1', 'Daney@live.co.uk', '1-1-1994', '30-03-2008', 'hr-155-42.hd-185-2.ch-255-62.lg-280-64.sh-300-62.ca-1815-72.wa-2009-62', 'M', '', '', '0', '0', '0,', '30-3-2008 10:48:54', null, '0', null, '91.108.106.76');
INSERT INTO `users_badges` VALUES ('1', 'XM8');
INSERT INTO `users_badges` VALUES ('2', 'ADM');
INSERT INTO `users_badges` VALUES ('2', 'HC2');
INSERT INTO `users_badges` VALUES ('7', 'SFK');
INSERT INTO `users_bans` VALUES ('9999', '', '11-4-2012 17:46:58', 'Optiefen kudthond. =]');
INSERT INTO `users_club` VALUES ('2', '0', '3', '24-2-2008');
INSERT INTO `users_club` VALUES ('1', '0', '3', '24-2-2008');
INSERT INTO `vouchers` VALUES ('888', '888');
INSERT INTO `wordfilter` VALUES ('aaron');
INSERT INTO `wordfilter` VALUES ('anal');
INSERT INTO `wordfilter` VALUES ('ass');
INSERT INTO `wordfilter` VALUES ('cunt');
INSERT INTO `wordfilter` VALUES ('dick');
INSERT INTO `wordfilter` VALUES ('fuck');
INSERT INTO `wordfilter` VALUES ('goddamn');
INSERT INTO `wordfilter` VALUES ('motherfucker');
INSERT INTO `wordfilter` VALUES ('pussy');
