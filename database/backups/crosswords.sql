PGDMP                         z         
   crosswords    15.1    15.1 @    R           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            S           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            T           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            U           1262    16505 
   crosswords    DATABASE     ~   CREATE DATABASE crosswords WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'Russian_Russia.1251';
    DROP DATABASE crosswords;
                postgres    false            �            1259    16689    crossword_words    TABLE     �   CREATE TABLE public.crossword_words (
    crossword_id smallint NOT NULL,
    word_id integer NOT NULL,
    x1 smallint NOT NULL,
    y1 smallint NOT NULL,
    x2 smallint NOT NULL,
    y2 smallint NOT NULL
);
 #   DROP TABLE public.crossword_words;
       public         heap    postgres    false            �            1259    16602 
   crosswords    TABLE     /  CREATE TABLE public.crosswords (
    crossword_id smallint NOT NULL,
    crossword_name character varying(30) NOT NULL,
    theme_id smallint NOT NULL,
    dictionary_id smallint NOT NULL,
    horizontal_size smallint NOT NULL,
    vertical_size smallint NOT NULL,
    prompt_count smallint NOT NULL
);
    DROP TABLE public.crosswords;
       public         heap    postgres    false            �            1259    16601    crosswords_crossword_id_seq    SEQUENCE     �   CREATE SEQUENCE public.crosswords_crossword_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public.crosswords_crossword_id_seq;
       public          postgres    false    221            V           0    0    crosswords_crossword_id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE public.crosswords_crossword_id_seq OWNED BY public.crosswords.crossword_id;
          public          postgres    false    220            �            1259    16527    dictionaries    TABLE     ~   CREATE TABLE public.dictionaries (
    dictionary_id smallint NOT NULL,
    dictionary_name character varying(30) NOT NULL
);
     DROP TABLE public.dictionaries;
       public         heap    postgres    false            �            1259    16526    dictionaries_dictionary_id_seq    SEQUENCE     �   CREATE SEQUENCE public.dictionaries_dictionary_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public.dictionaries_dictionary_id_seq;
       public          postgres    false    219            W           0    0    dictionaries_dictionary_id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE public.dictionaries_dictionary_id_seq OWNED BY public.dictionaries.dictionary_id;
          public          postgres    false    218            �            1259    16635    letters    TABLE     �   CREATE TABLE public.letters (
    player_id integer NOT NULL,
    crossword_id smallint NOT NULL,
    x smallint NOT NULL,
    y smallint NOT NULL,
    letter_name character(1) NOT NULL
);
    DROP TABLE public.letters;
       public         heap    postgres    false            �            1259    16507    players    TABLE     �   CREATE TABLE public.players (
    player_id integer NOT NULL,
    login character varying(10) NOT NULL,
    password_hash character(84) NOT NULL
);
    DROP TABLE public.players;
       public         heap    postgres    false            �            1259    16506    players_player_id_seq    SEQUENCE     �   CREATE SEQUENCE public.players_player_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public.players_player_id_seq;
       public          postgres    false    215            X           0    0    players_player_id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public.players_player_id_seq OWNED BY public.players.player_id;
          public          postgres    false    214            �            1259    16620    saves    TABLE     �   CREATE TABLE public.saves (
    player_id integer NOT NULL,
    crossword_id smallint NOT NULL,
    prompt_count smallint NOT NULL
);
    DROP TABLE public.saves;
       public         heap    postgres    false            �            1259    16645    solved_crosswords    TABLE     n   CREATE TABLE public.solved_crosswords (
    player_id integer NOT NULL,
    crossword_id smallint NOT NULL
);
 %   DROP TABLE public.solved_crosswords;
       public         heap    postgres    false            �            1259    16517    themes_theme_id_seq    SEQUENCE     �   CREATE SEQUENCE public.themes_theme_id_seq
    AS smallint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public.themes_theme_id_seq;
       public          postgres    false            �            1259    16518    themes    TABLE     �   CREATE TABLE public.themes (
    theme_id smallint DEFAULT nextval('public.themes_theme_id_seq'::regclass) NOT NULL,
    theme_name character varying(30) NOT NULL
);
    DROP TABLE public.themes;
       public         heap    postgres    false    216            �            1259    16661    words    TABLE     �   CREATE TABLE public.words (
    word_id integer NOT NULL,
    dictionary_id smallint NOT NULL,
    word_name character varying(15) NOT NULL,
    definition character varying(200) NOT NULL
);
    DROP TABLE public.words;
       public         heap    postgres    false            �            1259    16660    words_word_id_seq    SEQUENCE     �   CREATE SEQUENCE public.words_word_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public.words_word_id_seq;
       public          postgres    false    226            Y           0    0    words_word_id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public.words_word_id_seq OWNED BY public.words.word_id;
          public          postgres    false    225            �           2604    16605    crosswords crossword_id    DEFAULT     �   ALTER TABLE ONLY public.crosswords ALTER COLUMN crossword_id SET DEFAULT nextval('public.crosswords_crossword_id_seq'::regclass);
 F   ALTER TABLE public.crosswords ALTER COLUMN crossword_id DROP DEFAULT;
       public          postgres    false    220    221    221            �           2604    16530    dictionaries dictionary_id    DEFAULT     �   ALTER TABLE ONLY public.dictionaries ALTER COLUMN dictionary_id SET DEFAULT nextval('public.dictionaries_dictionary_id_seq'::regclass);
 I   ALTER TABLE public.dictionaries ALTER COLUMN dictionary_id DROP DEFAULT;
       public          postgres    false    219    218    219            �           2604    16510    players player_id    DEFAULT     v   ALTER TABLE ONLY public.players ALTER COLUMN player_id SET DEFAULT nextval('public.players_player_id_seq'::regclass);
 @   ALTER TABLE public.players ALTER COLUMN player_id DROP DEFAULT;
       public          postgres    false    214    215    215            �           2604    16664    words word_id    DEFAULT     n   ALTER TABLE ONLY public.words ALTER COLUMN word_id SET DEFAULT nextval('public.words_word_id_seq'::regclass);
 <   ALTER TABLE public.words ALTER COLUMN word_id DROP DEFAULT;
       public          postgres    false    225    226    226            O          0    16689    crossword_words 
   TABLE DATA           P   COPY public.crossword_words (crossword_id, word_id, x1, y1, x2, y2) FROM stdin;
    public          postgres    false    227   TO       I          0    16602 
   crosswords 
   TABLE DATA           �   COPY public.crosswords (crossword_id, crossword_name, theme_id, dictionary_id, horizontal_size, vertical_size, prompt_count) FROM stdin;
    public          postgres    false    221   �O       G          0    16527    dictionaries 
   TABLE DATA           F   COPY public.dictionaries (dictionary_id, dictionary_name) FROM stdin;
    public          postgres    false    219   �O       K          0    16635    letters 
   TABLE DATA           M   COPY public.letters (player_id, crossword_id, x, y, letter_name) FROM stdin;
    public          postgres    false    223   P       C          0    16507    players 
   TABLE DATA           B   COPY public.players (player_id, login, password_hash) FROM stdin;
    public          postgres    false    215   #P       J          0    16620    saves 
   TABLE DATA           F   COPY public.saves (player_id, crossword_id, prompt_count) FROM stdin;
    public          postgres    false    222   @P       L          0    16645    solved_crosswords 
   TABLE DATA           D   COPY public.solved_crosswords (player_id, crossword_id) FROM stdin;
    public          postgres    false    224   ]P       E          0    16518    themes 
   TABLE DATA           6   COPY public.themes (theme_id, theme_name) FROM stdin;
    public          postgres    false    217   zP       N          0    16661    words 
   TABLE DATA           N   COPY public.words (word_id, dictionary_id, word_name, definition) FROM stdin;
    public          postgres    false    226   �P       Z           0    0    crosswords_crossword_id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('public.crosswords_crossword_id_seq', 2, true);
          public          postgres    false    220            [           0    0    dictionaries_dictionary_id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('public.dictionaries_dictionary_id_seq', 2, true);
          public          postgres    false    218            \           0    0    players_player_id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('public.players_player_id_seq', 26, true);
          public          postgres    false    214            ]           0    0    themes_theme_id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public.themes_theme_id_seq', 3, true);
          public          postgres    false    216            ^           0    0    words_word_id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public.words_word_id_seq', 2, true);
          public          postgres    false    225            �           2606    16693 $   crossword_words crossword_words_pkey 
   CONSTRAINT     u   ALTER TABLE ONLY public.crossword_words
    ADD CONSTRAINT crossword_words_pkey PRIMARY KEY (crossword_id, word_id);
 N   ALTER TABLE ONLY public.crossword_words DROP CONSTRAINT crossword_words_pkey;
       public            postgres    false    227    227            �           2606    16609 (   crosswords crosswords_crossword_name_key 
   CONSTRAINT     m   ALTER TABLE ONLY public.crosswords
    ADD CONSTRAINT crosswords_crossword_name_key UNIQUE (crossword_name);
 R   ALTER TABLE ONLY public.crosswords DROP CONSTRAINT crosswords_crossword_name_key;
       public            postgres    false    221            �           2606    16607    crosswords crosswords_pkey 
   CONSTRAINT     b   ALTER TABLE ONLY public.crosswords
    ADD CONSTRAINT crosswords_pkey PRIMARY KEY (crossword_id);
 D   ALTER TABLE ONLY public.crosswords DROP CONSTRAINT crosswords_pkey;
       public            postgres    false    221            �           2606    16534 -   dictionaries dictionaries_dictionary_name_key 
   CONSTRAINT     s   ALTER TABLE ONLY public.dictionaries
    ADD CONSTRAINT dictionaries_dictionary_name_key UNIQUE (dictionary_name);
 W   ALTER TABLE ONLY public.dictionaries DROP CONSTRAINT dictionaries_dictionary_name_key;
       public            postgres    false    219            �           2606    16532    dictionaries dictionaries_pkey 
   CONSTRAINT     g   ALTER TABLE ONLY public.dictionaries
    ADD CONSTRAINT dictionaries_pkey PRIMARY KEY (dictionary_id);
 H   ALTER TABLE ONLY public.dictionaries DROP CONSTRAINT dictionaries_pkey;
       public            postgres    false    219            �           2606    16639    letters letters_pkey 
   CONSTRAINT     m   ALTER TABLE ONLY public.letters
    ADD CONSTRAINT letters_pkey PRIMARY KEY (player_id, crossword_id, x, y);
 >   ALTER TABLE ONLY public.letters DROP CONSTRAINT letters_pkey;
       public            postgres    false    223    223    223    223            �           2606    16516    players players_login_key 
   CONSTRAINT     U   ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_login_key UNIQUE (login);
 C   ALTER TABLE ONLY public.players DROP CONSTRAINT players_login_key;
       public            postgres    false    215            �           2606    16514    players players_pkey 
   CONSTRAINT     Y   ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_pkey PRIMARY KEY (player_id);
 >   ALTER TABLE ONLY public.players DROP CONSTRAINT players_pkey;
       public            postgres    false    215            �           2606    16624    saves saves_pkey 
   CONSTRAINT     c   ALTER TABLE ONLY public.saves
    ADD CONSTRAINT saves_pkey PRIMARY KEY (player_id, crossword_id);
 :   ALTER TABLE ONLY public.saves DROP CONSTRAINT saves_pkey;
       public            postgres    false    222    222            �           2606    16649 (   solved_crosswords solved_crosswords_pkey 
   CONSTRAINT     {   ALTER TABLE ONLY public.solved_crosswords
    ADD CONSTRAINT solved_crosswords_pkey PRIMARY KEY (player_id, crossword_id);
 R   ALTER TABLE ONLY public.solved_crosswords DROP CONSTRAINT solved_crosswords_pkey;
       public            postgres    false    224    224            �           2606    16523    themes themes_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public.themes
    ADD CONSTRAINT themes_pkey PRIMARY KEY (theme_id);
 <   ALTER TABLE ONLY public.themes DROP CONSTRAINT themes_pkey;
       public            postgres    false    217            �           2606    16525    themes themes_theme_name_key 
   CONSTRAINT     ]   ALTER TABLE ONLY public.themes
    ADD CONSTRAINT themes_theme_name_key UNIQUE (theme_name);
 F   ALTER TABLE ONLY public.themes DROP CONSTRAINT themes_theme_name_key;
       public            postgres    false    217            �           2606    16668 '   words words_dictionary_id_word_name_key 
   CONSTRAINT     v   ALTER TABLE ONLY public.words
    ADD CONSTRAINT words_dictionary_id_word_name_key UNIQUE (dictionary_id, word_name);
 Q   ALTER TABLE ONLY public.words DROP CONSTRAINT words_dictionary_id_word_name_key;
       public            postgres    false    226    226            �           2606    16666    words words_pkey 
   CONSTRAINT     S   ALTER TABLE ONLY public.words
    ADD CONSTRAINT words_pkey PRIMARY KEY (word_id);
 :   ALTER TABLE ONLY public.words DROP CONSTRAINT words_pkey;
       public            postgres    false    226            �           2606    16699 1   crossword_words crossword_words_crossword_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.crossword_words
    ADD CONSTRAINT crossword_words_crossword_id_fkey FOREIGN KEY (crossword_id) REFERENCES public.crosswords(crossword_id) ON DELETE CASCADE;
 [   ALTER TABLE ONLY public.crossword_words DROP CONSTRAINT crossword_words_crossword_id_fkey;
       public          postgres    false    3229    227    221            �           2606    16694 ,   crossword_words crossword_words_word_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.crossword_words
    ADD CONSTRAINT crossword_words_word_id_fkey FOREIGN KEY (word_id) REFERENCES public.words(word_id);
 V   ALTER TABLE ONLY public.crossword_words DROP CONSTRAINT crossword_words_word_id_fkey;
       public          postgres    false    227    226    3239            �           2606    16615 (   crosswords crosswords_dictionary_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.crosswords
    ADD CONSTRAINT crosswords_dictionary_id_fkey FOREIGN KEY (dictionary_id) REFERENCES public.dictionaries(dictionary_id);
 R   ALTER TABLE ONLY public.crosswords DROP CONSTRAINT crosswords_dictionary_id_fkey;
       public          postgres    false    3225    219    221            �           2606    16610 #   crosswords crosswords_theme_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.crosswords
    ADD CONSTRAINT crosswords_theme_id_fkey FOREIGN KEY (theme_id) REFERENCES public.themes(theme_id);
 M   ALTER TABLE ONLY public.crosswords DROP CONSTRAINT crosswords_theme_id_fkey;
       public          postgres    false    217    221    3219            �           2606    16640 +   letters letters_player_id_crossword_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.letters
    ADD CONSTRAINT letters_player_id_crossword_id_fkey FOREIGN KEY (player_id, crossword_id) REFERENCES public.saves(player_id, crossword_id) ON DELETE CASCADE;
 U   ALTER TABLE ONLY public.letters DROP CONSTRAINT letters_player_id_crossword_id_fkey;
       public          postgres    false    222    223    3231    222    223            �           2606    16630    saves saves_crossword_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.saves
    ADD CONSTRAINT saves_crossword_id_fkey FOREIGN KEY (crossword_id) REFERENCES public.crosswords(crossword_id) ON DELETE CASCADE;
 G   ALTER TABLE ONLY public.saves DROP CONSTRAINT saves_crossword_id_fkey;
       public          postgres    false    3229    222    221            �           2606    16625    saves saves_player_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.saves
    ADD CONSTRAINT saves_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(player_id) ON DELETE CASCADE;
 D   ALTER TABLE ONLY public.saves DROP CONSTRAINT saves_player_id_fkey;
       public          postgres    false    3217    222    215            �           2606    16655 5   solved_crosswords solved_crosswords_crossword_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.solved_crosswords
    ADD CONSTRAINT solved_crosswords_crossword_id_fkey FOREIGN KEY (crossword_id) REFERENCES public.crosswords(crossword_id) ON DELETE CASCADE;
 _   ALTER TABLE ONLY public.solved_crosswords DROP CONSTRAINT solved_crosswords_crossword_id_fkey;
       public          postgres    false    224    3229    221            �           2606    16650 2   solved_crosswords solved_crosswords_player_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.solved_crosswords
    ADD CONSTRAINT solved_crosswords_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(player_id) ON DELETE CASCADE;
 \   ALTER TABLE ONLY public.solved_crosswords DROP CONSTRAINT solved_crosswords_player_id_fkey;
       public          postgres    false    215    3217    224            �           2606    16669    words words_dictionary_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.words
    ADD CONSTRAINT words_dictionary_id_fkey FOREIGN KEY (dictionary_id) REFERENCES public.dictionaries(dictionary_id) ON DELETE CASCADE;
 H   ALTER TABLE ONLY public.words DROP CONSTRAINT words_dictionary_id_fkey;
       public          postgres    false    226    3225    219            O      x�3�4�4 BSN.CN#0������ 3�f      I   =   x�3�0�bÅ}/6^��.lQ0�4�4�44�44�4�2¦Ĉ����9�b���� Ew�      G   &   x�3估����.l���b��C.#t!#�=... !�I      K      x������ � �      C      x������ � �      J      x������ � �      L      x������ � �      E      x�3⼰���{.lP0�2Fp��b���� ���      N   C   x�3�4估����.l��Ȟwa�ņ[/l��@��[.6B�lP0�2B�cD�#�=... �TB     