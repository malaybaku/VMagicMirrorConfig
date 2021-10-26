using NUnit.Framework;

namespace Baku.VMagicMirrorConfig.Test
{
    public class UpdateDataTests
    {
        [Test]
        public void Test_�o�[�W�����l�p�[�X_����n()
        {
            var success = VmmAppVersion.TryParse("v1.2.3", out var result);
            Assert.IsTrue(success);
            Assert.AreEqual(1, result.Major);
            Assert.AreEqual(2, result.Minor);
            Assert.AreEqual(3, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�p�[�X_����n_�`����v�͖����Ă�����()
        {
            var success = VmmAppVersion.TryParse("4.0.2", out var result);
            Assert.IsTrue(success);
            Assert.AreEqual(4, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(2, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�p�[�X_����n_4���\�L����Ǝ�O��3�����g��()
        {
            var success = VmmAppVersion.TryParse("4.6.9.3", out var result);
            Assert.IsTrue(success);
            Assert.AreEqual(4, result.Major);
            Assert.AreEqual(6, result.Minor);
            Assert.AreEqual(9, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�p�[�X_�ُ�n_prefix��v�ȊO�̓_��()
        {
            var success = VmmAppVersion.TryParse("a1.2.3", out var result);
            Assert.IsFalse(success);
            Assert.AreEqual(0, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(0, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�p�[�X_�ُ�n_suffix�������Ă͂����Ȃ�()
        {
            var success = VmmAppVersion.TryParse("1.2.3a", out var result);
            Assert.IsFalse(success);
            Assert.AreEqual(0, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(0, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�p�[�X_�ُ�n_�r���ɂ��������l������ƃ_��()
        {
            var success = VmmAppVersion.TryParse("1.xxx.2", out var result);
            Assert.IsFalse(success);
            Assert.AreEqual(0, result.Major);
            Assert.AreEqual(0, result.Minor);
            Assert.AreEqual(0, result.Build);
        }

        [Test]
        public void Test_�o�[�W�����l�̔�r()
        {
            //���W���[�o�[�W�����Ō��܂�
            var a = new VmmAppVersion(2, 3, 4);
            var b = new VmmAppVersion(1, 5, 11);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //�}�C�i�[�o�[�W�����Ō��܂�
            b = new VmmAppVersion(2, 1, 5);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //�r���h�o�[�W�����Ō��܂�
            b = new VmmAppVersion(2, 3, 2);
            Assert.IsTrue(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));

            //������: �ǂ�����Newer�ł͂Ȃ�
            b = new VmmAppVersion(2, 3, 4);
            Assert.IsFalse(a.IsNewerThan(b));
            Assert.IsFalse(b.IsNewerThan(a));
        }

        [Test]
        public void Test_�o�[�W�����l��Valid�()
        {
            Assert.IsTrue(new VmmAppVersion(0, 0, 1).IsValid);
            Assert.IsTrue(new VmmAppVersion(0, 1, 0).IsValid);
            Assert.IsTrue(new VmmAppVersion(1, 0, 0).IsValid);
            //�S��0����NG
            Assert.IsFalse(new VmmAppVersion(0, 0, 0).IsValid);
        }

        [Test]
        public void Test_�����[�X�m�[�g����n()
        {
            var note = ReleaseNote.FromRawString(
@"2021/10/24

Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
");

            Assert.AreEqual("2021/10/24", note.DateString);
            Assert.AreEqual("- �ǉ�: hoge.\n- �C��: fuga.", note.JapaneseNote);
            Assert.AreEqual("- Add: Foo\n- Fix: Bar", note.EnglishNote);
        }

        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_���t�͏����Y���Ƌ�ɂȂ�()
        {
            var note = ReleaseNote.FromRawString(
@"
Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
");

            Assert.AreEqual("", note.DateString);
            Assert.AreEqual("- �ǉ�: hoge.\n- �C��: fuga.", note.JapaneseNote);
            Assert.AreEqual("- Add: Foo\n- Fix: Bar", note.EnglishNote);
        }

        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_���t�Ɍ����邪���������������Ƌ�()
        {
            var note = ReleaseNote.FromRawString(
@"2048/May/11

Japanese:

- �ǉ�: hoge.
- �C��: fuga.

English:

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
");

            Assert.AreEqual("", note.DateString);
            Assert.AreEqual("- �ǉ�: hoge.\n- �C��: fuga.", note.JapaneseNote);
            Assert.AreEqual("- Add: Foo\n- Fix: Bar", note.EnglishNote);
        }

        [Test]
        public void Test_�����[�X�m�[�g�ُ�n_��؂肪������JP��EN���S������()
        {
            var raw = 
@"2021/10/24

Japanese

- �ǉ�: hoge.
- �C��: fuga.

English

- Add: Foo
- Fix: Bar

Note:

- This is note area which should be ignored in parse process.
";
            var note = ReleaseNote.FromRawString(raw);

            //�����ɓ��t������ꍇ�͑S���̂ق��ɓ����Ă�΂����̂ŁADateString������ɂȂ��Ă�̂����A�Ƃ����̂��|�C���g
            Assert.AreEqual("", note.DateString);
            Assert.AreEqual(raw, note.JapaneseNote);
            Assert.AreEqual(raw, note.EnglishNote);
        }
    }
}