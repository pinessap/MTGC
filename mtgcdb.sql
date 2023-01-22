BEGIN;


DROP TABLE IF EXISTS public.battle CASCADE;

CREATE TABLE IF NOT EXISTS public.battle
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    player1 character varying COLLATE pg_catalog."default",
    player2 character varying COLLATE pg_catalog."default",
    battlelog character varying COLLATE pg_catalog."default",
    "timestamp" timestamp with time zone DEFAULT now(),
    CONSTRAINT battle_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public.card CASCADE;

CREATE TABLE IF NOT EXISTS public.card
(
    id character varying COLLATE pg_catalog."default" NOT NULL,
    name character varying COLLATE pg_catalog."default" NOT NULL,
    damage double precision NOT NULL,
    element character varying COLLATE pg_catalog."default",
    type character varying COLLATE pg_catalog."default",
    owner character varying COLLATE pg_catalog."default",
    packageid integer,
    CONSTRAINT card_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public."package" CASCADE;

CREATE TABLE IF NOT EXISTS public."package"
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    CONSTRAINT package_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public.trade CASCADE;

CREATE TABLE IF NOT EXISTS public.trade
(
    id character varying COLLATE pg_catalog."default" NOT NULL,
    "cardToTrade" character varying COLLATE pg_catalog."default" NOT NULL,
    type character varying COLLATE pg_catalog."default",
    damage character varying COLLATE pg_catalog."default",
    owner character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT trade_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public."user" CASCADE;

CREATE TABLE IF NOT EXISTS public."user"
(
    username character varying COLLATE pg_catalog."default" NOT NULL,
    password character varying COLLATE pg_catalog."default" NOT NULL,
    name character varying COLLATE pg_catalog."default",
    bio character varying COLLATE pg_catalog."default",
    image character varying COLLATE pg_catalog."default",
    coins integer,
    elo integer,
    wins integer,
    losses integer,
    "numGames" integer,
    card1 character varying COLLATE pg_catalog."default",
    card2 character varying COLLATE pg_catalog."default",
    card3 character varying COLLATE pg_catalog."default",
    card4 character varying COLLATE pg_catalog."default",
    CONSTRAINT user_pkey PRIMARY KEY (username),
    CONSTRAINT user_username_key UNIQUE (username)
);

ALTER TABLE IF EXISTS public.battle
    ADD CONSTRAINT battle_player1_fkey FOREIGN KEY (player1)
    REFERENCES public."user" (username) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS public.battle
    ADD CONSTRAINT battle_player2_fkey FOREIGN KEY (player2)
    REFERENCES public."user" (username) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS public.card
    ADD CONSTRAINT card_owner_fkey FOREIGN KEY (owner)
    REFERENCES public."user" (username) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
    NOT VALID;


ALTER TABLE IF EXISTS public.card
    ADD CONSTRAINT card_packageid_fkey FOREIGN KEY (packageid)
    REFERENCES public."package" (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
    NOT VALID;


ALTER TABLE IF EXISTS public.trade
    ADD CONSTRAINT "trade_cardToTrade_fkey" FOREIGN KEY ("cardToTrade")
    REFERENCES public.card (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS public.trade
    ADD CONSTRAINT trade_owner_fkey FOREIGN KEY (owner)
    REFERENCES public."user" (username) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS public."user"
    ADD CONSTRAINT user_card1_fkey FOREIGN KEY (card1)
    REFERENCES public.card (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
    NOT VALID;


ALTER TABLE IF EXISTS public."user"
    ADD CONSTRAINT user_card2_fkey FOREIGN KEY (card2)
    REFERENCES public.card (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
    NOT VALID;


ALTER TABLE IF EXISTS public."user"
    ADD CONSTRAINT user_card3_fkey FOREIGN KEY (card3)
    REFERENCES public.card (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
    NOT VALID;


ALTER TABLE IF EXISTS public."user"
    ADD CONSTRAINT user_card4_fkey FOREIGN KEY (card4)
    REFERENCES public.card (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
    NOT VALID;

END;
